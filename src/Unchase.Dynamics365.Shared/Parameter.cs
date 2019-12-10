namespace Unchase.Dynamics365.Shared
{
    public class Parameter
    {
        public readonly static string Target = nameof(Target);


        public readonly static string LeadIn = nameof(LeadIn);


        public readonly static string EntityMoniker = nameof(EntityMoniker);


        public readonly static string OpportunityClose = nameof(OpportunityClose);


        public readonly static string ContactId = nameof(ContactId);


        public readonly static string ErrorCode = nameof(ErrorCode);


        public readonly static string ErrorCodeString = nameof(ErrorCodeString);


        public readonly static string ErrorMessage = nameof(ErrorMessage);


        public readonly static string Relationship = nameof(Relationship);


        public readonly static string RelatedEntities = nameof(RelatedEntities);


        /// <summary>
        /// A <c>String</c> that specifies the unique name of the solution to which 
        /// the operation applies.
        /// </summary>
        public readonly static string SolutionUniqueName = nameof(SolutionUniqueName);


        /// <summary>
        /// A <c>Boolean</c> used to disable duplicate detection on a create or update 
        /// operation.
        /// </summary>
        public readonly static string SuppressDuplicateDetection = nameof(SuppressDuplicateDetection);
    }
}
