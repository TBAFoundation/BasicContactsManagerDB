using System.ComponentModel;

namespace ContactManager;

public enum ContactType
{
    [Description("Family and Friends")]
    FamilyAndFriends = 1,

    [Description("Work or Business")]
    WorkOrBusiness
}