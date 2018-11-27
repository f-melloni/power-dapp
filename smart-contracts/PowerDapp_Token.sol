pragma solidity 0.4.23;

import "./Zeppelin/token/ERC20/StandardToken.sol";
import "./Zeppelin/ownership/Ownable.sol";

import "./PowerDapp_KYC.sol";

/**
*  @notice PowerDapp Token is a standard ERC20 token. KYC verifications, dividend distribution and voting are in separate contracts.
*/
contract PowerDapp_Token is StandardToken, Ownable {
    string public name = "PowerDapp";
    string public symbol = "PowerDapp";
    uint8 public decimals = 0;

    PowerDapp_KYC public kycContract;

    event Burn(address indexed burner, uint256 value);

    /**
     * @notice The KYC contract can be upgraded by the owner, it doesn't require redeploying the token.
     * 
     * @param _kycContractAddress address - the address where PowerDapp_KYC contract is deployed
     */
    function setKycContract(address _kycContractAddress) public onlyOwner {
        kycContract = PowerDapp_KYC(_kycContractAddress);
    }

    /**
     * @notice This is a method for the initial distribution of tokens to the investors.
     * @notice All investors go though KYC verification before they purchase tokens in the ICO,
     * @notice so their verification status is automatically set to true.
     * 
     * @param _investors address[] - array of investor addresses
     * @param _values uint256[] - amount of tokens to send to each investor
     */
    function multisendAndVerify(address[] _investors, uint256[] _values) public onlyOwner returns (bool success) {
        for(uint i = 0; i < _investors.length; i++) {
            transfer(_investors[i], _values[i]);
            kycContract.KYC_confirm(_investors[i]);
        }

        return true;
    }

    /**
     * @notice Only owner can burn tokens. Unused tokens will be burned after the ICO ends.
     * 
     * @param _value uint256 - the amount of tokens to be burned
     */
    function burn(uint256 _value) public onlyOwner {
        _burn(msg.sender, _value);
    }

    function _burn(address _who, uint256 _value) internal {
        require(_value <= balances[_who]);
        // no need to require value <= totalSupply, since that would imply the
        // sender's balance is greater than the totalSupply, which *should* be an assertion failure

        balances[_who] = balances[_who].sub(_value);
        totalSupply_ = totalSupply_.sub(_value);
        emit Burn(_who, _value);
        emit Transfer(_who, address(0), _value);
    }
}
