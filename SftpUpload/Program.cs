using Microsoft.Extensions.Configuration;
using System.Reflection;
using WinSCP;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

ConnectionInfo info = config.GetRequiredSection("ConnectionInfo").Get<ConnectionInfo>()
    ?? throw new ArgumentNullException("ConnectionInfo is not exist!");

foreach (PropertyInfo propertyInfo in info.GetType().GetProperties())
{
    if (!propertyInfo.CanRead)
        throw new ArgumentNullException($"The config file is not exist {propertyInfo.Name} property!");
}

SessionOptions sessionOptions = new SessionOptions
{
    Protocol = Protocol.Sftp,
    HostName = info.HostName,
    UserName = info.UserName,
    Password = info.Password,
    SshHostKeyFingerprint = info.SshHostKeyFingerprint
};

using (Session session = new Session())
{
    session.Open(sessionOptions);

    TransferOptions transferOptions = new TransferOptions();
    transferOptions.TransferMode = TransferMode.Automatic;

    var t = DateTime.Now;
    string uploadFolderName = $"{t.Year}年{t.Month}月{t.Day}日 {t.Hour}时{t.Minute}分{t.Second}秒";
    uploadFolderName = $"{info.RemotePath}/{uploadFolderName}";

    TransferOperationResult transferResult;
    transferResult = session.PutFiles(info.LocalPath, uploadFolderName, false, transferOptions);

    transferResult.Check();

    foreach (TransferEventArgs transfer in transferResult.Transfers)
    {
        Console.WriteLine($"Upload of {transfer.FileName} succeeded");
    }
}

return 0;

public sealed class ConnectionInfo
{
    public string? HostName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? SshHostKeyFingerprint { get; set; }
    public string? LocalPath { get; set; }
    public string? RemotePath { get; set; }
}
