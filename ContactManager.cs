using ConsoleTables;
using Humanizer;

namespace ContactManager;

internal sealed class ContactManager : IContactManager
{
    private const string ContactsFilePath = "contacts.txt";
    public static List<Contact> Contacts = new();
    
    public ContactManager()
    {
        LoadContactsFromFile();
    }
    public void AddContact(string name, string phoneNumber, string? email, ContactType contactType)
    {
        try
        {
            var isContactExist = IsContactExist(phoneNumber);

            if (IsContactExist(phoneNumber))
            {
                throw new ContactException($"Contact {phoneNumber} already exists.");
            }

            int id = Contacts.Count > 0 ? Contacts.Count + 1 : 1;

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

            SaveContactsToFile();
        }
        catch (ContactException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while adding the contact: " + ex.Message);
        }
    }
    public void DeleteContact(string phoneNumber)
    {
        try
        {
            var contact = FindContact(phoneNumber);

            if (contact is null)
            {
                throw new ContactException($"Unable to delete as contact with '{phoneNumber}' not found.");
            }

            Contacts.Remove(contact);
            Console.WriteLine("Contact deleted successfully.");

            SaveContactsToFile();
        }
        catch (ContactException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while deleting the contact: " + ex.Message);
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
            Console.WriteLine("An error occurred while finding the contact: " + ex.Message);
            return null;
        }
    }


    public void GetContact(string phoneNumber)
    {
        try
        {
            var contact = FindContact(phoneNumber);

            if (contact is null)
            {
                throw new ContactException($"Contact with {phoneNumber} not found");
            }

            Print(contact);
        }
        catch (ContactException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while getting the contact: " + ex.Message);
        }
    }

    public void GetAllContacts()
    {
        try
        {
            int contactCount = Contacts.Count;

            Console.WriteLine("You have " + "contact".ToQuantity(contactCount));

            if (contactCount == 0)
            {
                Console.WriteLine("There are no contacts added yet.");
                return;
            }

            var table = new ConsoleTable("Id", "Name", "Phone Number", "Email", "Contact Type", "Date Created");

            foreach (var contact in Contacts)
            {
                table.AddRow(contact.Id, contact.Name, contact.PhoneNumber, contact.Email, contact.ContactType.Humanize(), contact.CreatedAt.Humanize());
            }

            table.Write(Format.Alternative);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while retrieving contacts: " + ex.Message);
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

        SaveContactsToFile();
    }
    catch (ContactException ex)
    {
        Console.WriteLine(ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred while updating the contact: " + ex.Message);
    }
}

    private void Print(Contact contact)
    {
        Console.WriteLine($"Name: {contact!.Name}\nPhone Number: {contact!.PhoneNumber}\nEmail: {contact!.Email}");
    }

    private bool IsContactExist(string phoneNumber)
    {
        try
        {
            return Contacts.Exists(c => c.PhoneNumber == phoneNumber);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while checking contact existence: " + ex.Message);
            return false;
        }
    }

    private void LoadContactsFromFile()
    {
        try
        {
            if (File.Exists(ContactsFilePath))
            {
                using (StreamReader reader = new StreamReader(ContactsFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()!) != null)
                    {
                        Contact contact = Contact.FromString(line);
                        Contacts.Add(contact);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while loading contacts from file: " + ex.Message);
        }
    }

    private void SaveContactsToFile()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(ContactsFilePath))
            {
                writer.WriteLine("Id\tName\tPhone Number\tEmail\tContact Type\tDate Created");

                foreach (var contact in Contacts)
                {
                    string line = $"{contact.Id}\t{contact.Name}\t{contact.PhoneNumber}\t{contact.Email}\t{contact.ContactType.Humanize()}\t{contact.CreatedAt.Humanize()}";
                    writer.WriteLine(line);
                }
            }
            // using (StreamWriter writer = new StreamWriter(ContactsFilePath))
            // {
            //     foreach (var contact in Contacts)
            //     {
            //         string line = $"Id:{contact.Id}, Name:{contact.Name}, Phone number:{contact.PhoneNumber}, Email:{contact.Email}, Contact Type: {contact.ContactType.Humanize()}, Date Created:{contact.CreatedAt.Humanize()}";
            //         writer.WriteLine(line);
            //     }
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred while saving contacts to file: " + ex.Message);
        }
    }
}