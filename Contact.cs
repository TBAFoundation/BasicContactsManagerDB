namespace ContactManager;
public class Contact : BaseClass
{
    public string Name { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public ContactType ContactType { get; set; }

    public static Contact FromString(string line)
    {
        var contactData = line.Split(',');

        if (contactData.Length < 6)
        {
            throw new ArgumentException("Invalid contact data: " + line);
        }

        var contact = new Contact
        {
            Id = int.Parse(contactData[0]),
            Name = contactData[1],
            PhoneNumber = contactData[2],
            Email = contactData[3],
            ContactType = Enum.Parse<ContactType>(contactData[4]),
            CreatedAt = DateTime.Parse(contactData[5])
        };

        return contact;
    }
}