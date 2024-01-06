using ConsoleTables;
using Humanizer;

namespace ContactManager;

internal sealed class ContactManager : IContactManager
{
    // public static List<Contact> Contacts = new();
    private static readonly string FilePath = "contacts.txt";
    private static List<Contact> Contacts = new();

    static ContactManager()
    {
        LoadContacts();
    }

    private static void LoadContacts()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                using (StreamReader reader = new StreamReader(FilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()!) != null)
                    {
                        Contact contact = DeserializeContact(line);
                        Contacts.Add(contact);
                    }
                }
            }
        }
        catch (FileNotFoundException)
        {
            
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error loading contacts: {ex.Message}", ex);
        }
    }

    private static void SaveContacts()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(FilePath, true))
            {
                foreach (var contact in Contacts)
                {
                    string line = SerializeContact(contact);
                    writer.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error saving contacts: {ex.Message}", ex);
        }
    }
    private static void RefreshContacts()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(FilePath))
            {
                foreach (var contact in Contacts)
                {
                    string line = SerializeContact(contact);
                    writer.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error saving contacts: {ex.Message}", ex);
        }
    }

    private static Contact DeserializeContact(string line)
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
        throw new ContactException("Invalid contact format in the file.");
    }

    private static string SerializeContact(Contact contact)
    {
        return $"{contact.Id}|{contact.Name}|{contact.PhoneNumber}|{contact.Email}|{(int)contact.ContactType}|{contact.CreatedAt}";
    }
    public void AddContact(string name, string phoneNumber, string? email, ContactType contactType)
    {
        try
        {
            int id = Contacts.Count > 0 ? Contacts.Max(c => c.Id) + 1 : 1;

            var isContactExist = IsContactExist(phoneNumber);

            if (isContactExist)
            {
                throw new ContactException("Contact already exists!");
            }

            var contact = new Contact
            {
                Id = id,
                Name = name,
                PhoneNumber = phoneNumber,
                Email = email,
                ContactType = contactType,
                CreatedAt = DateTime.Now
            };

            Contacts.Add(contact);
            Console.WriteLine("Contact added successfully.");
            SaveContacts();
        }
        catch (ContactException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error adding contact: {ex.Message}", ex);
        }
    }

    public void DeleteContact(string phoneNumber)
    {
        try
        {
            var contact = FindContact(phoneNumber);

            if (contact is null)
            {
                throw new ContactException("Unable to delete contact as it does not exist!");
            }

            Contacts.Remove(contact);
            RefreshContacts();
        }
        catch (ContactException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error deleting contact: {ex.Message}", ex);
        }
    }
    public Contact? FindContact(string phoneNumber)
    {
        try
        {
            return Contacts.Find(c => c.PhoneNumber == phoneNumber);
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error finding contact: {ex.Message}", ex);
        }
    }

    public void GetContact(string phoneNumber)
    {
        try
        {
            var contact = FindContact(phoneNumber);

            if (contact is null)
            {
                Console.WriteLine($"Contact with {phoneNumber} not found");
            }
            else
            {
                Print(contact);
            }
        }
        catch (ContactException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error getting contact: {ex.Message}", ex);
        }
    }
    public void GetAllContacts()
    {
        try
        {
            // LoadContacts();

            int contactCount = Contacts.Count;

            Console.WriteLine("You have " + "contact".ToQuantity(contactCount));

            if (contactCount == 0)
            {
                Console.WriteLine("There is no contact added yet.");
                return;
            }

            var table = new ConsoleTable("Id", "Name", "Phone Number", "Email", "Contact Type", "Date Created");

            foreach (var contact in Contacts)
            {
                table.AddRow(contact.Id, contact.Name, contact.PhoneNumber, contact.Email, ((ContactType)contact.ContactType).Humanize(), contact.CreatedAt.Humanize());
            }

            table.Write(Format.Alternative);
        }
        catch (ContactException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error getting all contacts: {ex.Message}", ex);
        }
    }

    public void UpdateContact(string phoneNumber, string name, string email)
    {
        try
        {
            var contact = FindContact(phoneNumber);

            if (contact is null)
            {
                throw new ContactException("Contact does not exist!");
            }

            contact.Name = name;
            contact.Email = email;
            Console.WriteLine("Contact updated successfully.");
            RefreshContacts();
        }
        catch (ContactException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ContactException($"Error updating contact: {ex.Message}", ex);
        }
    }

    private void Print(Contact contact)
    {
        Console.WriteLine($"Name: {contact!.Name}\nPhone Number: {contact!.PhoneNumber}\nEmail: {contact!.Email}");
    }

    private bool IsContactExist(string phoneNumber)
    {
        return Contacts.Exists(c => c.PhoneNumber == phoneNumber);
    }
}