namespace Maneuver.LocalData.Cryptography
{
	public interface ICryptographer
	{
		string Decrypt(byte[] soup, string key);
		byte[] Encrypt(string original, string key);
	}
}