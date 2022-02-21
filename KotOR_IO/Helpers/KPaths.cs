using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotOR_IO.Helpers
{
    /// <summary>
    /// Contains paths to the various subdirectories within the game directory.
    /// </summary>
    public class KPaths
    {
        /// <summary> Path to the swkotor game directory. </summary>
        public readonly string swkotor;
        /// <summary> Path to the chitin.key file within the swkotor directory. </summary>
        public readonly string chitin;
        /// <summary> Path to the swkotor\data directory. </summary>
        public readonly string data;
        /// <summary> Path to the swkotor\lips directory. </summary>
        public readonly string lips;
        /// <summary> Path to the swkotor\modules directory. </summary>
        public readonly string modules;
        /// <summary> Path to the swkotor\Override directory. </summary>
        public readonly string Override;
        /// <summary> Path to the swkotor\rims directory. </summary>
        public readonly string rims;
        /// <summary> Path to the swkotor\streammusic directory. </summary>
        public readonly string music;
        /// <summary> Path to the swkotor\streamsounds directory. </summary>
        public readonly string sounds;
        /// <summary> Path to the swkotor\TexturePacks directory. </summary>
        public readonly string TexturePacks;
        /// <summary> Path to the dialog.tlk file within the swkotor directory </summary>
        public readonly string dialog;

        /// <summary> Path to the RANDOMIZED.log file within the swkotor directory. </summary>
        public readonly string RANDOMIZED_LOG;
        /// <summary> Filename of the log file indicating that the game has been randomized. </summary>
        public const string RANDOMIZED_LOG_FILENAME = "RANDOMIZED.log";

        /// <summary> Path to the backup of the chitin.key file within the swkotor directory. </summary>
        public readonly string chitin_backup;
        /// <summary> Path to the backup of the swkotor\data directory. </summary>
        public readonly string data_backup;
        /// <summary> Path to the backup of the swkotor\lips directory. </summary>
        public readonly string lips_backup;
        /// <summary> Path to the backup of the swkotor\modules directory. </summary>
        public readonly string modules_backup;
        /// <summary> Path to the backup of the swkotor\Override directory. </summary>
        public readonly string Override_backup;
        /// <summary> Path to the backup of the swkotor\rims directory. </summary>
        public readonly string rims_backup;
        /// <summary> Path to the backup of the swkotor\streammusic directory. </summary>
        public readonly string music_backup;
        /// <summary> Path to the backup of the swkotor\streamsounds directory. </summary>
        public readonly string sounds_backup;
        /// <summary> Path to the backup of the swkotor\TexturePacks directory. </summary>
        public readonly string TexturePacks_backup;
        /// <summary> Path to the backup of the dialog.tlk file within the swkotor directory. </summary>
        public readonly string dialog_backup;

        /// <summary>
        /// Constructs paths to the SW KotOR directory and subdirectories.
        /// </summary>
        /// <param name="swkotor_path">Path to the base swkotor game directory.</param>
        public KPaths(string swkotor_path)
        {
            swkotor = $"{swkotor_path}\\";
            chitin = $"{swkotor_path}\\chitin.key";
            data = $"{swkotor_path}\\data\\";
            lips = $"{swkotor_path}\\lips\\";
            modules = $"{swkotor_path}\\modules\\";
            Override = $"{swkotor_path}\\Override\\";
            rims = $"{swkotor_path}\\rims\\";
            music = $"{swkotor_path}\\streammusic\\";
            sounds = $"{swkotor_path}\\streamsounds\\";
            TexturePacks = $"{swkotor_path}\\TexturePacks\\";
            dialog = $"{swkotor_path}\\dialog.tlk";

            RANDOMIZED_LOG = $"{swkotor_path}\\{RANDOMIZED_LOG_FILENAME}";

            chitin_backup = $"{swkotor_path}\\chitin.key.bak";
            data_backup = $"{swkotor_path}\\data_bak\\";
            lips_backup = $"{swkotor_path}\\lips_bak\\";
            modules_backup = $"{swkotor_path}\\modules_bak\\";
            Override_backup = $"{swkotor_path}\\Override_bak\\";
            rims_backup = $"{swkotor_path}\\rims_bak\\";
            music_backup = $"{swkotor_path}\\streammusic_bak\\";
            sounds_backup = $"{swkotor_path}\\streamsounds_bak\\";
            TexturePacks_backup = $"{swkotor_path}\\TexturePacks_bak\\";
            dialog_backup = $"{swkotor_path}\\dialog.tlk.bak";
        }

        /// <summary> Returns a list of the current files in the swkotor base directory. </summary>
        public FileInfo[] FilesInBaseDir => new DirectoryInfo(swkotor).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\data directory. </summary>
        public FileInfo[] FilesInData => new DirectoryInfo(data).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\lips directory. </summary>
        public FileInfo[] FilesInLips => new DirectoryInfo(lips).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\modules directory. </summary>
        public FileInfo[] FilesInModules => new DirectoryInfo(modules).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\music directory. </summary>
        public FileInfo[] FilesInMusic => new DirectoryInfo(music).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\music_bak directory. </summary>
        public FileInfo[] FilesInMusicBackup => new DirectoryInfo(music_backup).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\sounds directory. </summary>
        public FileInfo[] FilesInSounds => new DirectoryInfo(sounds).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\sounds_bak directory. </summary>
        public FileInfo[] FilesInSoundsBackup => new DirectoryInfo(sounds_backup).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\Override directory. </summary>
        public FileInfo[] FilesInOverride => new DirectoryInfo(Override).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\rims directory. </summary>
        public FileInfo[] FilesInRims => new DirectoryInfo(rims).GetFiles();

        /// <summary> Returns a list of the current files in the swkotor\TexturePacks directory. </summary>
        public FileInfo[] FilesInTexturePacks => new DirectoryInfo(TexturePacks).GetFiles();

        /// <summary>
        /// Creates a backup of the modules directory if it doesn't exist already.
        /// </summary>
        public void BackUpModulesDirectory()
        {
            if (!Directory.Exists(modules_backup))
            {
                Directory.CreateDirectory(modules_backup);
                foreach (FileInfo file in FilesInModules)
                {
                    file.CopyTo(Path.Combine(modules_backup, file.Name), true);
                }
            }
        }

        /// <summary>
        /// Creates a backup of the lips directory if it doesn't exist already.
        /// </summary>
        public void BackUpLipsDirectory()
        {
            if (!Directory.Exists(lips_backup))
            {
                Directory.CreateDirectory(lips_backup);
                foreach (FileInfo file in FilesInLips)
                {
                    file.CopyTo(Path.Combine(lips_backup, file.Name), true);
                }
            }
        }

        /// <summary>
        /// Creates a backup of the Override directory if it doesn't exist already.
        /// </summary>
        public void BackUpOverrideDirectory()
        {
            if (!Directory.Exists(Override_backup))
            {
                Directory.CreateDirectory(Override_backup);
                foreach (FileInfo file in FilesInOverride)
                {
                    file.CopyTo(Path.Combine(Override_backup, file.Name), true);
                }
            }
        }

        /// <summary>
        /// Creates a backup of the chitin file if it doesn't exist already.
        /// </summary>
        public void BackUpChitinFile()
        {
            if (!File.Exists(chitin_backup))
            {
                File.Copy(chitin, chitin_backup);
            }
        }

        /// <summary>
        /// Creates a backup of the music directory if it doesn't exist already.
        /// </summary>
        public void BackUpMusicDirectory()
        {
            if (!Directory.Exists(music_backup))
            {
                Directory.CreateDirectory(music_backup);
                foreach (FileInfo file in FilesInMusic)
                {
                    file.CopyTo(Path.Combine(music_backup, file.Name), true);
                }
            }
        }

        /// <summary>
        /// Creates a backup of the sound directory if it doesn't exist already.
        /// </summary>
        public void BackUpSoundDirectory()
        {
            if (!Directory.Exists(sounds_backup))
            {
                Directory.CreateDirectory(sounds_backup);
                foreach (FileInfo file in FilesInSounds)
                {
                    file.CopyTo(Path.Combine(sounds_backup, file.Name), true);
                }
            }
        }

        /// <summary>
        /// Creates a backup of the TexturePacks directory if it doesn't exist already.
        /// </summary>
        public void BackUpTexturesDirectory()
        {
            if (!Directory.Exists(TexturePacks_backup))
            {
                Directory.CreateDirectory(TexturePacks_backup);
                foreach (FileInfo file in FilesInTexturePacks)
                {
                    file.CopyTo(Path.Combine(TexturePacks_backup, file.Name), true);
                }
            }
        }

        /// <summary>
        /// Creates a backup of the dialog file if it doesn't exist already.
        /// </summary>
        public void BackUpDialogFile()
        {
            if (!File.Exists(dialog_backup))
            {
                File.Copy(dialog, dialog_backup);
            }
        }

        /// <summary>
        /// If the backup Modules directory exists, restore it to the active directory.
        /// </summary>
        public void RestoreModulesDirectory()
        {
            if (Directory.Exists(modules_backup))
            {
                if (Directory.Exists(modules))
                    Directory.Delete(modules, true);
                Directory.Move(modules_backup, modules);
            }
        }

        /// <summary>
        /// If the backup Lips directory exists, restore it to the active directory.
        /// </summary>
        public void RestoreLipsDirectory()
        {
            if (Directory.Exists(lips_backup))
            {
                if (Directory.Exists(lips))
                    Directory.Delete(lips, true);
                Directory.Move(lips_backup, lips);
            }
        }

        /// <summary>
        /// If the backup Override directory exists, restore it to the active directory.
        /// </summary>
        public void RestoreOverrideDirectory()
        {
            if (Directory.Exists(Override_backup))
            {
                if (Directory.Exists(Override))
                    Directory.Delete(Override, true);
                Directory.Move(Override_backup, Override);
            }
        }

        /// <summary>
        /// If the backup Music directory exists, restore it to the active directory.
        /// </summary>
        public void RestoreMusicDirectory()
        {
            if (Directory.Exists(music_backup))
            {
                if (Directory.Exists(music))
                    Directory.Delete(music, true);
                Directory.Move(music_backup, music);
            }
        }

        /// <summary>
        /// If the backup Sounds directory exists, restore it to the active directory.
        /// </summary>
        public void RestoreSoundsDirectory()
        {
            if (Directory.Exists(sounds_backup))
            {
                if (Directory.Exists(sounds))
                    Directory.Delete(sounds, true);
                Directory.Move(sounds_backup, sounds);
            }
        }

        /// <summary>
        /// If the backup TexturePacks directory exists, restore it to the active directory.
        /// </summary>
        public void RestoreTexturePacksDirectory()
        {
            if (Directory.Exists(TexturePacks_backup))
            {
                if (Directory.Exists(TexturePacks))
                    Directory.Delete(TexturePacks, true);
                Directory.Move(TexturePacks_backup, TexturePacks);
            }
        }

        /// <summary>
        /// If the backup chitin file exists, restore it to the active directory.
        /// </summary>
        public void RestoreChitinFile()
        {
            if (File.Exists(chitin_backup))
            {
                if (File.Exists(chitin))
                    File.Delete(chitin);
                File.Move(chitin_backup, chitin);
            }
        }

        /// <summary>
        /// If the backup dialog file exists, restore it to the active directory.
        /// </summary>
        public void RestoreDialogFile()
        {
            if (File.Exists(dialog_backup))
            {
                if (File.Exists(dialog))
                    File.Delete(dialog);
                File.Move(dialog_backup, dialog);
            }
        }

        /// <summary>
        /// Gets the backup version of the requested path.
        /// </summary>
        /// <param name="path">Path to a directory or file.</param>
        /// <returns>Path to the backup version.</returns>
        public static string GetBackupPath(string path)
        {
            return path.Last() == '\\'
                ? path.TrimEnd('\\') + "_bak\\"
                : path + ".bak";
        }

        /// <summary>
        /// Gets the old version of the requested path.
        /// </summary>
        /// <param name="path">Path to a directory or file.</param>
        /// <returns>Path to the old version.</returns>
        public static string GetOldPath(string path)
        {
            return path.Last() == '\\'
                ? path.TrimEnd('\\') + "_old\\"
                : path + ".old";
        }

        /// <summary>
        /// Prints the path to the base directory for KotOR.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Base directory: {swkotor}";
        }
    }
}
