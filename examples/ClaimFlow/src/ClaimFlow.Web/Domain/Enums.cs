namespace ClaimFlow.Web.Domain;

public enum ClaimType
{
    Unknown = 0,
    Osago = 1,
    Kasko = 2
}

public enum ClaimStatus
{
    Draft = 0,
    Received = 1,
    Validating = 2,
    Assessing = 3,
    WaitingForDocuments = 4,
    ManualReview = 5,
    Approved = 6,
    Rejected = 7,
    Paid = 8,
    Failed = 9,
    Cancelled = 10
}

public enum FinalDecisionType
{
    None = 0,
    Approve = 1,
    Reject = 2,
    RequestDocuments = 3,
    ManualReview = 4
}
