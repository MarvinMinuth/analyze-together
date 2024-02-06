using System.IO;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    [SerializeField] private string folderName;
    private bool filesAreInPersistentDataPath = false;

    private void Awake()
    {
        EnsureFolderExistsInPersistentDataPath(folderName);
    }

    private void EnsureFolderExistsInPersistentDataPath(string folder)
    {
        string destinationFolderPath = Path.Combine(Application.persistentDataPath, folder);

        // Überprüfen, ob der Ordner im persistentDataPath existiert.
        if (!Directory.Exists(destinationFolderPath))
        {
            // Erstellen des Ordners im persistentDataPath.
            Directory.CreateDirectory(destinationFolderPath);

            // Laden Sie alle Dateien aus dem Ordner im Resources-Verzeichnis.
            TextAsset[] assets = Resources.LoadAll<TextAsset>(folder);

            // Jede Datei im Resources-Ordner in den persistentDataPath kopieren.
            foreach (TextAsset asset in assets)
            {
                string destinationFilePath = Path.Combine(destinationFolderPath, asset.name + ".json"); // Fügt die ".json"-Erweiterung hinzu.
                File.WriteAllText(destinationFilePath, asset.text);
            }
        }
        filesAreInPersistentDataPath = true;
    }

    public bool FilesAreReady()
    {
        return filesAreInPersistentDataPath;
    }

    public string getFolderName()
    {
        return folderName;
    }
}
