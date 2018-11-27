pragma solidity 0.4.23;

import "./PowerDapp_Token.sol";

/**
 * @notice Voting based on token ownership.
 */
contract PowerDappVoting_BallotInstance {
    
    // This declares a new complex type which will
    // be used for variables later.
    // It will represent a single voter.
    struct Voter {
        bool voted;  // if true, that person already voted
        uint vote;   // index of the voted proposal
    }

    // Proposal is one possible answer to a ballot question.
    struct Proposal {
        bytes32 name;   // short name (up to 32 bytes)
        uint voteCount; // number of accumulated votes
    }

    event Vote(uint proposalId, uint weight);
  
    PowerDapp_Token public token_contract;

    // This maps a voter to each address that participated in the ballot.
    mapping(address => Voter) public voters;

    // List of proposals (possible answers).
    Proposal[] public proposals;

    function getProposalCount() public view returns (uint256 count) {
        return proposals.length;
    }

    bytes public ballotQuestion;
    uint public ballotStartTime;
    uint public ballotEndTime;

    /**
     * @notice Constructor - create a new ballot.
     * 
     * @param _tokenAddress address - the address where PowerDapp_Token is deployed
     * @param _question bytes - ballot question
     * @param _timeLimitInDays uint - number of days until ballot is closed
     * @param _proposalNames bytes32[] - list of ballot proposals
     */
    constructor (address _tokenAddress, bytes _question, uint _timeLimitInDays, bytes32[] _proposalNames) public {
        token_contract = PowerDapp_Token(_tokenAddress);
        ballotQuestion = _question;
        ballotStartTime = now;
        ballotEndTime = now + (_timeLimitInDays * (1 days));

        // For each of the provided proposal names, create a new proposal object 
        // and add it to the end of the array.
        for (uint i = 0; i < _proposalNames.length; i++) {
            proposals.push(Proposal({
                name: _proposalNames[i],
                voteCount: 0
            }));
        }
    }

    /**
     * @notice Cast your vote for a particlar proposal. Voting power is directly proportinal to the amount of PowerDapp tokens owned.
     * @notice Each address can vote only once per ballot.
     * 
     * @param _proposal uint - index of proposal you vote for
     */
    function vote(uint _proposal) public {
        // check if ballot is still open
        require(now <= ballotEndTime);
    
        // check if user owns PowerDapp tokens
        uint tokensOwned = token_contract.balanceOf(msg.sender);
        require(tokensOwned>0);

        // check that user didn't vote yet
        Voter storage sender = voters[msg.sender];
        require(!sender.voted);
    
        sender.voted = true;
        sender.vote = _proposal;

        // If `proposal` is out of the range of the array, 
        // this will throw automatically and revert all changes.
        proposals[_proposal].voteCount += tokensOwned;

        emit Vote(_proposal, tokensOwned);
    }

    /**
     * @notice Computes the winning proposal taking all previous votes into account.
     *  
     * @return uint
     */ 
    function winningProposal() public view 
        returns (uint winningProposalResult)
    {
        uint winningVoteCount = 0;
        for (uint p = 0; p < proposals.length; p++) {
            if (proposals[p].voteCount > winningVoteCount) {
                winningVoteCount = proposals[p].voteCount;
                winningProposalResult = p;
            }
        }
    }

    /**
     * @notice Method Calls winningProposal() function to get the index of the winner 
     * @notice and then returns the text of the proposal
     * 
     * @return bytes32
     */
    function winnerName() public view
        returns (bytes32 winnerNameResult)
    {
        winnerNameResult = proposals[winningProposal()].name;
    }
}
