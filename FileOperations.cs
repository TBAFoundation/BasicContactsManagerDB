namespace ContactManager;

internal class FileOperations
{
    private readonly string FilePath;

    public FileOperations(string filePath)
    {
        FilePath = filePath;
    }

    public List<Contact> LoadContacts()
    {
        List<Contact> contacts = new List<Contact>();

        try
        {
            using (StreamReader reader = new StreamReader(FilePath))
            {
                string line;
                while ((line = reader.ReadLine()!) != null)
                {
                    Contact contact = DeserializeContact(line);
                    contacts.Add(contact);
                }
            }
        }
        catch (Exception ex)
        {
            HandleContactException($"Error loading contacts from file: {ex.Message}", ex);
        }

        return contacts;
    }

    public void SaveContacts(List<Contact> contacts)
    {
        try
        {
            // Clear existing data in the file
            ClearFile();

            // Write new data to the file
            using (StreamWriter writer = new StreamWriter(FilePath, true))
            {
                foreach (var contact in contacts)
                {
                    string line = SerializeContact(contact);
                    writer.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            HandleContactException($"Error saving contacts to file: {ex.Message}", ex);
        }
    }

    private void ClearFile()
    {
        try
        {
            // Clear the file
            using (StreamWriter writer = new StreamWriter(FilePath, false))
            {
                // Do nothing, just open and close to clear the file
            }
        }
        catch (Exception ex)
        {
            HandleContactException($"Error clearing file: {ex.Message}", ex);
        }
    }

    private Contact DeserializeContact(string line)
    {
        string[] parts = line.Split('|');
        if (parts.Length == 6)
        {
            return new Contact
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
                PhoneNumber = parts[2],
                Email = parts[3],
                ContactType = (ContactType)Enum.Parse(typeof(ContactType), parts[4]),
                CreatedAt = DateTime.Parse(parts[5])
            };
        }
        HandleContactException("Invalid contact format in the file.", null);
        return new Contact(); // Unreachable, but needed for compiler
    }

    private string SerializeContact(Contact contact)
    {
        return $"{contact.Id}|{contact.Name}|{contact.PhoneNumber}|{contact.Email}|{(int)contact.ContactType}|{contact.CreatedAt}";
    }

    private static void HandleContactException(string message, Exception? ex)
    {
        throw new ContactException(message, ex ?? new Exception());
    }
}


// internal class FileOperations
// {
//     private readonly string FilePath;

//     public FileOperations(string filePath)
//     {
//         FilePath = filePath;
//     }

//     public List<Contact> LoadContacts()
//     {
//         List<Contact> contacts = new List<Contact>();

//         try
//         {
//             using (StreamReader reader = new StreamReader(FilePath))
//             {
//                 string line;
//                 while ((line = reader.ReadLine()!) != null)
//                 {
//                     Contact contact = DeserializeContact(line);
//                     contacts.Add(contact);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error loading contacts from file: {ex.Message}", ex);
//         }

//         return contacts;
//     }

//     public void SaveContacts(List<Contact> contacts)
//     {
//         try
//         {
//             using (StreamWriter writer = new StreamWriter(FilePath, true))
//             {
//                 foreach (var contact in contacts)
//                 {
//                     string line = SerializeContact(contact);
//                     writer.WriteLine(line);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error saving contacts to file: {ex.Message}", ex);
//         }
//     }

//     public void RefreshContacts(List<Contact> contacts)
//     {
//         try
//         {
//             using (StreamWriter writer = new StreamWriter(FilePath))
//             {
//                 foreach (var contact in contacts)
//                 {
//                     string line = SerializeContact(contact);
//                     writer.WriteLine(line);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error refreshing contacts in file: {ex.Message}", ex);
//         }
//     }

//     private Contact DeserializeContact(string line)
//     {
//         string[] parts = line.Split('|');
//         if (parts.Length == 6)
//         {
//             return new Contact
//             {
//                 Id = int.Parse(parts[0]),
//                 Name = parts[1],
//                 PhoneNumber = parts[2],
//                 Email = parts[3],
//                 ContactType = (ContactType)Enum.Parse(typeof(ContactType), parts[4]),
//                 CreatedAt = DateTime.Parse(parts[5])
//             };
//         }
//         throw new ContactException("Invalid contact format in the file.");
//     }

//     private string SerializeContact(Contact contact)
//     {
//         return $"{contact.Id}|{contact.Name}|{contact.PhoneNumber}|{contact.Email}|{(int)contact.ContactType}|{contact.CreatedAt}";
//     }
// }
