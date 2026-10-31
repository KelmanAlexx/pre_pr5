using System.Security.Cryptography;

if (args.Length < 1)
{
    PrintHelp();
    return;
}

try
{
    var command = args[0].ToLowerInvariant();

    if (command == "backup" && args.Length == 3)
    {
        BackupDirectory(args[1], args[2]);
    }
    else if (command == "install" && args.Length == 4)
    {
        InstallRelease(args[1], args[2], args[3]);
    }
    else if (command == "rollback" && args.Length == 3)
    {
        Rollback(args[1], args[2]);
    }
    else
    {
        PrintHelp();
    }
}
catch (Exception exception)
{
    Console.WriteLine("Ошибка: " + exception.Message);
    Environment.ExitCode = 1;
}

static void BackupDirectory(string source, string backup)
{
    if (!Directory.Exists(source))
        throw new DirectoryNotFoundException("Каталог приложения не найден.");

    if (Directory.Exists(backup))
        throw new IOException("Каталог резервной копии уже существует.");

    CopyDirectory(source, backup);
    Console.WriteLine("Резервная копия создана: " + Path.GetFullPath(backup));
}

static void InstallRelease(string release, string app, string backup)
{
    if (!Directory.Exists(release))
        throw new DirectoryNotFoundException("Каталог релиза не найден.");

    if (!Directory.Exists(app))
        throw new DirectoryNotFoundException("Каталог приложения не найден.");

    // Сначала сохраняем текущую версию. Если backup уже существует,
    // установка не продолжается, чтобы не перезаписать резервную копию.
    BackupDirectory(app, backup);

    var temporary = app + ".new";
    if (Directory.Exists(temporary))
        Directory.Delete(temporary, true);

    CopyDirectory(release, temporary);

    var old = app + ".old";
    if (Directory.Exists(old))
        Directory.Delete(old, true);

    Directory.Move(app, old);

    try
    {
        Directory.Move(temporary, app);
        Console.WriteLine("Новая версия установлена.");
    }
    catch
    {
        if (Directory.Exists(app))
            Directory.Delete(app, true);

        Directory.Move(old, app);
        throw;
    }
}

static void Rollback(string backup, string app)
{
    if (!Directory.Exists(backup))
        throw new DirectoryNotFoundException("Резервная копия не найдена.");

    var failed = app + ".failed";
    if (Directory.Exists(failed))
        Directory.Delete(failed, true);

    if (Directory.Exists(app))
        Directory.Move(app, failed);

    try
    {
        CopyDirectory(backup, app);
        Console.WriteLine("Предыдущая версия восстановлена.");
    }
    catch
    {
        if (Directory.Exists(app))
            Directory.Delete(app, true);

        if (Directory.Exists(failed))
            Directory.Move(failed, app);

        throw;
    }
}

static void CopyDirectory(string source, string destination)
{
    Directory.CreateDirectory(destination);

    foreach (var file in Directory.GetFiles(source))
    {
        var target = Path.Combine(destination, Path.GetFileName(file));
        File.Copy(file, target, false);
    }

    foreach (var directory in Directory.GetDirectories(source))
    {
        var target = Path.Combine(destination, Path.GetFileName(directory));
        CopyDirectory(directory, target);
    }
}

static void PrintHelp()
{
    Console.WriteLine("CampusUpdater (.NET 8)");
    Console.WriteLine("backup <каталог_приложения> <каталог_резервной_копии>");
    Console.WriteLine("install <каталог_релиза> <каталог_приложения> <каталог_резервной_копии>");
    Console.WriteLine("rollback <каталог_резервной_копии> <каталог_приложения>");
}
