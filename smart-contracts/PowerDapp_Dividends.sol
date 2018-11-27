pragma solidity 0.4.23;

import "./Zeppelin/math/SafeMath.sol";

import "./PowerDapp_Token.sol";

contract PowerDapp_Dividends {
    using SafeMath for uint256;

    PowerDapp_Token public tokenContract;

    /**
     * @notice This modifier requires the sender to be owner of the Token contract.
     */
    modifier onlyTokenOwner {
        require(msg.sender == tokenContract.owner());
        _;
    }

    event DistributeDividends(uint amountSent, uint totalDividends);

    /**
     * @notice Constructor
     * 
     * @param _tokenAddress address - the address where PowerDapp_Token is deployed
     */
    constructor(address _tokenAddress) public {
        tokenContract = PowerDapp_Token(_tokenAddress);
    }

    uint256 nextPayeeIndex; // Used to mark where dividend distribution stopped, to prevent running out of gas.
    uint256 currentDividendAmount; // Set total amount of ETH to be distributed.
    uint256 actualDividendsDistributed; // Counts dividends that are distributed to KYC verified token holders.

    /**
     * @notice Called before distributing dividends. It sets the total amount to be distributed.
     * 
     * @param _amount uint256 - the amount that PowerDapp intends to distribute
     */
    function setDividendAmount(uint256 _amount) onlyTokenOwner public returns (bool success) {
        currentDividendAmount = _amount;
        actualDividendsDistributed = 0;
        return true;
    }

    /**
     * @notice Distributes the dividends. It may be called multiple times.
     * @notice If the nuber of verified investors is to large to cover in one trunsactin (gas runs out),
     * @notice the next call continues where the previous one left off.
     */
    function distributeDividends() payable onlyTokenOwner public returns (bool success) {
        require(currentDividendAmount > 0); // Distribution can start only when the amount to be distributed is set

        uint256 i = nextPayeeIndex; // Restore index in case gas ran out the last time
        uint256 ethRemaining = msg.value; // Count ETH to return to owner after the function call

        while(i < tokenContract.kycContract().kycAddressesLength() && gasleft() > 200000) {
            address user = tokenContract.kycContract().kycAddresses(i);
            uint userDividend = currentDividendAmount.mul(tokenContract.balanceOf(user)).div(tokenContract.totalSupply()); // use SafeMath methods imported above

            if(tokenContract.kycContract().kycIsVerified(user)) {
                actualDividendsDistributed = actualDividendsDistributed.add(userDividend); // Increase the total dividend amount that will be reported in DAPP
                ethRemaining = ethRemaining.sub(userDividend); // Decrease the unused amount that will be returned to owner
                user.transfer(userDividend);
            }
            i++;
        }
        nextPayeeIndex = i; // Save payee index to storage in case gas ran out and we need to continue

        if(nextPayeeIndex == tokenContract.kycContract().kycAddressesLength()) { // Round complete
            emit DistributeDividends(currentDividendAmount, actualDividendsDistributed); // Emit event for the DAPP
            currentDividendAmount = 0; // Require dividend amount to be set beofre the next distribution to prevent using old value
            nextPayeeIndex = 0; // Reset payee index for next round
        }

        tokenContract.owner().transfer(ethRemaining); // Return unused ETH to owner
        return true;
    }

    /**
     * @notice Checks if the dividend distribution is copleted, or another call is needed.
     */
    function divedendDistributionIsComplete() public view returns (bool complete) {
        return (nextPayeeIndex == 0);
    }

    /**
     * @notice This contract doesn't store ether, but in case the dividend process is not performed correctly, the owner can always reclaim any leftover ether.
     */
    function reclaimEther() external onlyTokenOwner {
        // solium-disable-next-line security/no-send
        assert(tokenContract.owner().send(address(this).balance));
    }
}
