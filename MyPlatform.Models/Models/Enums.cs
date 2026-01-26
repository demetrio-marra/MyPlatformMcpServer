namespace MyPlatformModels.Models
{
    public enum enumStatisticType
    {
        Nothing = 0,
        Create = 1,
        Modify = 2,
        Renew = 3,
        Deactivation = 4,
        Expiration = 5,
        Get = 6,
        GDPR = 7
    }

    public enum MyPlatform_Statistics_DataPartitioning
    {
        Year = 1,
        Month = 2,
        Day = 3
    }
}
