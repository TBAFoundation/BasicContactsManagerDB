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
                        string[] contactData = line.Split('|');
                        if (contactData.Length == 6)
                        {
                            Contacts.Add(new Contact
                            {
                                Id = int.Parse(contactData[0]),
                                Name = contactData[1],
                                PhoneNumber = contactData[2],
                                Email = contactData[3],
                                ContactType = (ContactType)Enum.Parse(typeof(ContactType), contactData[4]),
                                CreatedAt = DateTime.Parse(contactData[5])
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading contacts: {ex.Message}");
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
                    string line = $"{contact.Id}|{contact.Name}|{contact.PhoneNumber}|{contact.Email}|{(int)contact.ContactType}|{contact.CreatedAt:yyyy-MM-dd HH:mm:ss}";
                    writer.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving contacts: {ex.Message}");
        }
    }
    public void AddContact(string name, string phoneNumber, string? email, ContactType contactType)
    {
        int id = Contacts.Count > 0 ? Contacts.Count + 1 : 1;

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


    public void DeleteContact(string phoneNumber)
    {
        var contact = FindContact(phoneNumber);

        if (contact is null)
        {
            throw new ContactException("Unable to delete contact as it does not exist!");
        }

        Contacts.Remove(contact);
        SaveContacts();
    }

    public Contact? FindContact(string phoneNumber)
    {
        return Contacts.Find(c => c.PhoneNumber == phoneNumber);
    }

    public void GetContact(string phoneNumber)
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

    public void GetAllContacts()
    {
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

    public void UpdateContact(string phoneNumber, string name, string email)
    {
        var contact = FindContact(phoneNumber);

        if (contact is null)
        {
            throw new ContactException("Contact does not exist!");
        }

        contact.Name = name;
        contact.Email = email;
        Console.WriteLine("Contact updated successfully.");
        SaveContacts();
    }
    private void Print(Contact contact)
    {
        Console.WriteLine($"Name: {contact!.Name}\nPhone Number: {contact!.PhoneNumber}\nEmail: {contact!.Email}");
    }

    private bool IsContactExist(string phoneNumber)
    {
        return Contacts.Any(c => c.PhoneNumber == phoneNumber);
    }
}