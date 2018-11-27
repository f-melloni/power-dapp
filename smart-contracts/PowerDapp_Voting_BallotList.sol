pragma solidity 0.4.23;

import "./PowerDapp_Token.sol";

/**
 * @notice List of ballots for the PowerDapp frontend application.
 * @notice It is intentionally separated from the token so that the voting system cen be easily upgraded in the fututre.
 * @notice An address where this contract is deployed is configurad in the appliacation. 
 */
contract PowerDapp_Voting_BallotList {

    PowerDapp_Token public tokenContract;

    /** @var address[] ballotAddresses - list of ballot instances that were added to the ballot list */
    address[] public ballotAddresses;

    /** @var uit questionCount - count of PowerDapp_BallotInstances associated with this ballot list */
    uint public questionCount = 0;

    /**
     * @notice Return number of ballot instances on this list.
     */
    function questionCount() public view returns (uint count) {
        return ballotAddresses.length;
    }

    event CreateBallot(address BallotAddress);

    // User must hold at least 10% of all tokens to propose questions
    modifier mustHaveEnoughTokens {
        require((tokenContract.balanceOf(msg.sender) * 10) >= tokenContract.totalSupply());
        _;
    }

    /**
     * @param _tokenAddress address - the address where PowerDapp_Token is deployed
     */
    constructor(address _tokenAddress) public {
        tokenContract = PowerDapp_Token(_tokenAddress);
    }

    /**
     * @notice Add a ballot instance to the list. It will then appear in the frontend.
     * 
     * @param _ballotAddress address - address of the PowerDapp_BallotInstance
     * @return bool
     */
    function addBallot(address _ballotAddress) mustHaveEnoughTokens public returns (bool success) {
        ballotAddresses.push(_ballotAddress);
        emit CreateBallot(_ballotAddress);
        return true;
    }

    /**
     * @notice Get the list of ballot instance addresses.
     * 
     * @return address[] 
     */
    function getBallotAddresses() public view returns (address[] addresses) {
        return ballotAddresses;
    }
}
