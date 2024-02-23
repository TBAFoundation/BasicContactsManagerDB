using MySql.Data.MySqlClient;
using ConsoleTables;
using Humanizer;
using System.Data;

namespace ContactManager;

internal sealed class ContactManager : IContactManager
{
    private static readonly string ConnectionString = "Server=localhost;Database=MyContactsDB;User Id=root;Password=Dec**##2794;Port=3306;";
    private static readonly string FilePath = "contacts.txt";
    private static readonly FileOperations fileOperations = new FileOperations(FilePath);
    private static List<Contact> Contacts = new();

    static ContactManager()
    {
        LoadContacts();
    }

    private static void LoadContacts()
    {
        try
        {
            // Load contacts from the database
            Contacts = LoadContactsFromDatabase();

            // If no contacts in the database, try loading from the file
            if (Contacts.Count == 0)
            {
                Contacts = fileOperations.LoadContacts();
            }
        }
        catch (Exception ex)
        {
            HandleContactException($"Error loading contacts: {ex.Message}", ex);
        }
    }

    private static List<Contact> LoadContactsFromDatabase()
    {
        List<Contact> contacts = new List<Contact>();

        try
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM Contacts";
                using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Contact contact = new Contact
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            PhoneNumber = reader.GetString("PhoneNumber"),
                            Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                            ContactType = (ContactType)reader.GetInt32("ContactType"),
                            CreatedAt = reader.GetDateTime("CreatedAt")
                        };

                        contacts.Add(contact);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HandleContactException($"Error loading contacts from database: {ex.Message}", ex);
        }

        return contacts;
    }

    private static void SaveContacts()
    {
        try
        {
            // Save contacts to the file
            fileOperations.SaveContacts(Contacts);

            // Save contacts to the database
            SaveContactsToDatabase();
        }
        catch (Exception ex)
        {
            HandleContactException($"Error saving contacts: {ex.Message}", ex);
        }
    }

    private static void SaveContactsToDatabase()
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                // Clear existing data in the database
                string clearTableQuery = "TRUNCATE TABLE Contacts";
                using (MySqlCommand clearCommand = new MySqlCommand(clearTableQuery, connection))
                {
                    clearCommand.ExecuteNonQuery();
                }

                // Insert new data into the database
                foreach (var contact in Contacts)
                {
                    string insertQuery = "INSERT INTO Contacts (Name, PhoneNumber, Email, ContactType, CreatedAt) " +
                                        "VALUES (@Name, @PhoneNumber, @Email, @ContactType, @CreatedAt)";

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", contact.Name);
                        command.Parameters.AddWithValue("@PhoneNumber", contact.PhoneNumber);
                        command.Parameters.AddWithValue("@Email", contact.Email);
                        command.Parameters.AddWithValue("@ContactType", (int)contact.ContactType);
                        command.Parameters.AddWithValue("@CreatedAt", contact.CreatedAt);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HandleContactException($"Error saving contacts to database: {ex.Message}", ex);
        }
    }

    public void AddContact(string name, string phoneNumber, string? email, ContactType contactType)
    {
        try
        {
            Contact newContact = new Contact
            {
                Id = Contacts.Count + 1, // Assuming auto-increment is not used in the database
                Name = name,
                PhoneNumber = phoneNumber,
                Email = email,
                ContactType = contactType,
                CreatedAt = DateTime.Now
            };

            Contacts.Add(newContact);
            SaveContacts();

            Console.WriteLine("Contact added successfully.");
        }
        catch (Exception ex)
        {
            HandleContactException($"Error adding contact: {ex.Message}", ex);
        }
    }
    public void DeleteContact(string phoneNumber)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM Contacts WHERE PhoneNumber = @PhoneNumber";

                using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new ContactException("Unable to delete contact as it does not exist!");
                    }
                }
            }

            Console.WriteLine("Contact deleted successfully.");
        }
        catch (Exception ex)
        {
            HandleContactException($"Error deleting contact: {ex.Message}", ex);
        }
    }

    public Contact? FindContact(string phoneNumber)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM Contacts WHERE PhoneNumber = @PhoneNumber";

                using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Contact
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                                ContactType = (ContactType)reader.GetInt32("ContactType"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            };
                        }
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            HandleContactException($"Error finding contact: {ex.Message}", ex);
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
            HandleContactException($"Error getting contact: {ex.Message}", ex);
        }
    }

    public void GetAllContacts()
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM Contacts";

                using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    List<Contact> contacts = new List<Contact>();

                    while (reader.Read())
                    {
                        Contact contact = new Contact
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            PhoneNumber = reader.GetString("PhoneNumber"),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                            ContactType = (ContactType)reader.GetInt32("ContactType"),
                            CreatedAt = reader.GetDateTime("CreatedAt")
                        };

                        contacts.Add(contact);
                    }

                    int contactCount = contacts.Count;

                    Console.WriteLine($"You have {contactCount} contact{(contactCount == 1 ? "" : "s")}");

                    if (contactCount == 0)
                    {
                        Console.WriteLine("There are no contacts added yet.");
                        return;
                    }

                    var table = new ConsoleTable("Id", "Name", "Phone Number", "Email", "Contact Type", "Date Created");

                    foreach (var contact in contacts)
                    {
                        table.AddRow(contact.Id, contact.Name, contact.PhoneNumber, contact.Email, contact.ContactType.Humanize(), contact.CreatedAt.Humanize());
                    }

                    table.Write(Format.Alternative);
                }
            }
        }
        catch (Exception ex)
        {
            HandleContactException($"Error getting all contacts: {ex.Message}", ex);
        }
    }

    public void UpdateContact(string phoneNumber, string name, string email)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string updateQuery = "UPDATE Contacts SET Email = @Email WHERE PhoneNumber = @PhoneNumber";

                using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new ContactException("Unable to update contact as it does not exist!");
                    }
                }
            }

            Console.WriteLine("Contact updated successfully.");
        }
        catch (Exception ex)
        {
            HandleContactException($"Error updating contact: {ex.Message}", ex);
        }
    }

    private void Print(Contact contact)
    {
        Console.WriteLine($"Name: {contact.Name}\nPhone Number: {contact.PhoneNumber}\nEmail: {contact.Email}");
    }

    private bool IsContactExist(string phoneNumber)
    {
        return Contacts.Exists(c => c.PhoneNumber == phoneNumber);
    }

    private static void HandleContactException(string message, Exception ex)
    {
        throw new ContactException(message, ex);
    }
}

// internal sealed class ContactManager : IContactManager
// {
//     private static readonly string ConnectionString = "Server=localhost; Database=MyContactsDB; User Id=root;Password=Dec**##2794;Port=3306;";
//     private static readonly string FilePath = "contacts.txt";
//     private static readonly List<Contact> Contacts = new();

//     static ContactManager()
//     {
//         LoadContacts();
//     }
//     private static void LoadContacts()
//     {
//         try
//         {
//             // Assuming you already have a table named 'Contacts' in your MySQL database
//             using (MySqlConnection connection = new MySqlConnection(ConnectionString))
//             {
//                 connection.Open();

//                 string selectQuery = "SELECT * FROM Contacts";
//                 using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
//                 using (MySqlDataReader reader = command.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         Contact contact = new Contact
//                         {
//                             Id = reader.GetInt32("Id"),
//                             Name = reader.GetString("Name"),
//                             PhoneNumber = reader.GetString("PhoneNumber"),
//                             Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
//                             ContactType = (ContactType)Enum.Parse(typeof(ContactType), reader.GetInt32("ContactType").ToString()),
//                             CreatedAt = reader.GetDateTime("CreatedAt")
//                         };

//                         Contacts.Add(contact);
//                     }
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error loading contacts: {ex.Message}", ex);
//         }
//     }

//     private static void SaveContacts()
//     {
//         try
//         {
//             using (StreamWriter writer = new StreamWriter(FilePath, true))
//             {
//                 foreach (var contact in Contacts)
//                 {
//                     string line = SerializeContact(contact);
//                     writer.WriteLine(line);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error saving contacts: {ex.Message}", ex);
//         }
//     }

//     private static void RefreshContacts()
//     {
//         try
//         {
//             using (StreamWriter writer = new StreamWriter(FilePath))
//             {
//                 foreach (var contact in Contacts)
//                 {
//                     string line = SerializeContact(contact);
//                     writer.WriteLine(line);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error saving contacts: {ex.Message}", ex);
//         }
//     }

//     private static Contact DeserializeContact(string line)
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

//     private static string SerializeContact(Contact contact)
//     {
//         return $"{contact.Id}|{contact.Name}|{contact.PhoneNumber}|{contact.Email}|{(int)contact.ContactType}|{contact.CreatedAt}";
//     }
//     public void AddContact(string name, string phoneNumber, string? email, ContactType contactType)
//     {
//         try
//         {
//             using (MySqlConnection connection = new MySqlConnection(ConnectionString))
//             {
//                 connection.Open();

//                 string insertQuery = "INSERT INTO Contacts (Name, PhoneNumber, Email, ContactType, CreatedAt) " +
//                                     "VALUES (@Name, @PhoneNumber, @Email, @ContactType, @CreatedAt)";

//                 using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
//                 {
//                     command.Parameters.AddWithValue("@Name", name);
//                     command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
//                     command.Parameters.AddWithValue("@Email", email);
//                     command.Parameters.AddWithValue("@ContactType", (int)contactType);
//                     command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

//                     command.ExecuteNonQuery();
//                 }
//             }

//             Console.WriteLine("Contact added successfully.");
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error adding contact: {ex.Message}", ex);
//         }
//     }

//     public void DeleteContact(string phoneNumber)
//     {
//         try
//         {
//             using (MySqlConnection connection = new MySqlConnection(ConnectionString))
//             {
//                 connection.Open();

//                 string deleteQuery = "DELETE FROM Contacts WHERE PhoneNumber = @PhoneNumber";

//                 using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
//                 {
//                     command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

//                     int rowsAffected = command.ExecuteNonQuery();

//                     if (rowsAffected == 0)
//                     {
//                         throw new ContactException("Unable to delete contact as it does not exist!");
//                     }
//                 }
//             }

//             Console.WriteLine("Contact deleted successfully.");
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error deleting contact: {ex.Message}", ex);
//         }
//     }

//     public Contact? FindContact(string phoneNumber)
//     {
//         try
//         {
//             using (MySqlConnection connection = new MySqlConnection(ConnectionString))
//             {
//                 connection.Open();

//                 string selectQuery = "SELECT * FROM Contacts WHERE PhoneNumber = @PhoneNumber";

//                 using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
//                 {
//                     command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

//                     using (MySqlDataReader reader = command.ExecuteReader())
//                     {
//                         if (reader.Read())
//                         {
//                             return new Contact
//                             {
//                                 Id = reader.GetInt32("Id"),
//                                 Name = reader.GetString("Name"),
//                                 PhoneNumber = reader.GetString("PhoneNumber"),
//                                 Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
//                                 ContactType = (ContactType)Enum.Parse(typeof(ContactType), reader.GetInt32("ContactType").ToString()),
//                                 CreatedAt = reader.GetDateTime("CreatedAt")
//                             };
//                         }
//                     }
//                 }
//             }

//             return null;
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error finding contact: {ex.Message}", ex);
//         }
//     }

//     public void GetContact(string phoneNumber)
//     {
//         try
//         {
//             var contact = FindContact(phoneNumber);

//             if (contact is null)
//             {
//                 Console.WriteLine($"Contact with {phoneNumber} not found");
//             }
//             else
//             {
//                 Print(contact);
//             }
//         }
//         catch (ContactException)
//         {
//             throw;
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error getting contact: {ex.Message}", ex);
//         }
//     }
//     public void GetAllContacts()
//     {
//         try
//         {
//             using (MySqlConnection connection = new MySqlConnection(ConnectionString))
//             {
//                 connection.Open();

//                 string selectQuery = "SELECT * FROM Contacts";

//                 using (MySqlCommand command = new MySqlCommand(selectQuery, connection))
//                 using (MySqlDataReader reader = command.ExecuteReader())
//                 {
//                     List<Contact> contacts = new List<Contact>();

//                     while (reader.Read())
//                     {
//                         Contact contact = new Contact
//                         {
//                             Id = reader.GetInt32("Id"),
//                             Name = reader.GetString("Name"),
//                             PhoneNumber = reader.GetString("PhoneNumber"),
//                             Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
//                             ContactType = (ContactType)Enum.Parse(typeof(ContactType), reader.GetInt32("ContactType").ToString()),
//                             CreatedAt = reader.GetDateTime("CreatedAt")
//                         };

//                         contacts.Add(contact);
//                     }

//                     int contactCount = contacts.Count;

//                     Console.WriteLine("You have " + "contact".ToQuantity(contactCount));

//                     if (contactCount == 0)
//                     {
//                         Console.WriteLine("There are no contacts added yet.");
//                         return;
//                     }

//                     var table = new ConsoleTable("Id", "Name", "Phone Number", "Email", "Contact Type", "Date Created");

//                     foreach (var contact in contacts)
//                     {
//                         table.AddRow(contact.Id, contact.Name, contact.PhoneNumber, contact.Email, contact.ContactType.GetDescription(), contact.CreatedAt.Humanize());
//                     }

//                     table.Write(Format.Alternative);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error getting all contacts: {ex.Message}", ex);
//         }
//     }

//     public void UpdateContact(string phoneNumber, string name, string email)
//     {
//         try
//         {
//             using (MySqlConnection connection = new MySqlConnection(ConnectionString))
//             {
//                 connection.Open();

//                 string updateQuery = "UPDATE Contacts SET Email = @Email WHERE PhoneNumber = @PhoneNumber";

//                 using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
//                 {
//                     command.Parameters.AddWithValue("@Email", email);
//                     command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

//                     int rowsAffected = command.ExecuteNonQuery();

//                     if (rowsAffected == 0)
//                     {
//                         throw new ContactException("Unable to update contact as it does not exist!");
//                     }
//                 }
//             }

//             Console.WriteLine("Contact updated successfully.");
//         }
//         catch (Exception ex)
//         {
//             throw new ContactException($"Error updating contact: {ex.Message}", ex);
//         }
//     }

//     private void Print(Contact contact)
//     {
//         Console.WriteLine($"Name: {contact!.Name}\nPhone Number: {contact!.PhoneNumber}\nEmail: {contact!.Email}");
//     }
// }
//private bool IsContactExist(string phoneNumber)
//    {
//        return Contacts.Exists(c => c.PhoneNumber == phoneNumber);
//    }
