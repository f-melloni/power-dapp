pragma solidity 0.4.23;

import "./Zeppelin/ownership/Ownable.sol";
import "./PowerDapp_Token.sol";

/**
 *  @notice KYC verification for the PowerDapp token. Verification is required to receive dividends.
 */
contract PowerDapp_KYC {
    mapping (address => bool) public kycIsVerified;
    mapping (address => bool) private kycAddressUsed; // this prevents the same address being added multiple times
    address[] public kycAddresses;

    PowerDapp_Token public tokenContract; // reference to the token contract, used to check the owner and token holders

    event KYCConfirm(address user);
    event KYCRemove(address user);

    /**
     * @notice This modifier requires the sender to be owner of the Token contract.
     */
    modifier onlyTokenOwner {
        require(msg.sender == tokenContract.owner());
        _;
    }

    /**
     * @param _tokenAddress address - the address where PowerDapp_Token is deployed
     */
    constructor(address _tokenAddress) public {
        tokenContract = PowerDapp_Token(_tokenAddress);
    }

    /**
     * @notice Confirms that user successfully passed the KYC verification process.
     * 
     * @param _user address - address of the verified user
     */
    function KYC_confirm(address _user) onlyTokenOwner public returns (bool success) {

        // Optinal: Uncomment this line if you want to prevent verification of users that don't own tokens yet.
        // It was recommended in one of the audits, but seems counterproductive.
        
        // require(token_contract.balanceOf(user) > 0);

        kycIsVerified[_user] = true;
        if(!kycAddressUsed[_user]) { // this prevent duplicate entries
            kycAddresses.push(_user); // an array of addresses to scan when distributing dividends
            kycAddressUsed[_user] = true;
        }
    
        emit KYCConfirm(_user);
        return true;
    }

    /**
     * @notice Removes KYC verification for an address that was previously verified
     *
     * @param _user address - address of the user to remove
     */
    function KYC_remove(address _user) onlyTokenOwner public returns (bool success) {
        kycIsVerified[_user] = false;
    
        emit KYCRemove(_user);
        return true;
    }

    function kycAddressesLength() public view returns(uint count) {
        return kycAddresses.length;
    }
}
