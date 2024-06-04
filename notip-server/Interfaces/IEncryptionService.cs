namespace notip_server.Interfaces
{
    public interface IEncryptionService
    {
        string DecryptSymmetricKey(string encryptedSymmetricKey);

        string DecryptMessage(string encryptedMessage, string decryptedSymmetricKey);
    }
}
