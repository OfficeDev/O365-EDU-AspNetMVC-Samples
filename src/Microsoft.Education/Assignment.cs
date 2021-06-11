using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Education.Data
{
    public class Assignment
    {
        public Assignment()
        {
            Resources = new List<ResourceContainer>();
        }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("allowLateSubmissions")]
        public bool AllowLateSubmissions { get; set; }

        [JsonProperty("allowStudentsToAddResourcesToSubmission")]
        public bool AllowStudentsToAddResourcesToSubmission { get; set; }

        [JsonProperty("assignDateTime")]
        public string AssignDateTime { get; set; }

        [JsonProperty("assignedDateTime")]
        public string AssignedDateTime { get; set; }

        [JsonProperty("classId")]
        public string ClassId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("dueDateTime")]
        public string DueDateTime { get; set; }

        //[JsonProperty("lastModifiedBy")]
        //public string LastModifiedBy { get; set; }

        //[JsonProperty("lastModifiedDateTime")]
        //public DateTime LastModifiedDateTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }



        [JsonProperty("resources")]
        public List<ResourceContainer> Resources { get; set; }

        public string ResourceFiles { get; set; }

        public static string GetResourcesFiles(List<ResourceContainer> resources)
        {
            List<string> array = new List<string>();
            foreach (var item in resources)
            {
                if (item != null && item.Resource != null && !string.IsNullOrEmpty(item.Resource.DisplayName))
                {
                    array.Add(item.Resource.DisplayName);
                }
            }
            return string.Join(",", array);
        }

    }

    public class ResourceContainer
    {
        [JsonProperty("resource")]
        public EducationResource Resource { get; set; }
    }

    public class ResourcesFolder
    {
        [JsonProperty("odataid")]
        public string Odataid { get; set; }

        [JsonProperty("value")]
        public string ResourceFolderURL { get; set; }

    }

    public class EducationAssignmentResource
    {
        [JsonProperty("distributeForStudentWork")]
        public string DistributeForStudentWork { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resource")]
        public EducationResource Resource { get; set; }

    }

    public class EducationResource
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }
    }

    public class Submission
    {
        public Submission()
        {
            Resources = new List<EducationSubmissionResource>();
        }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("submittedDateTime")]
        public string SubmittedDateTime { get; set; }

        [JsonProperty("submittedBy")]
        public SubmittedBy SubmittedBy { get; set; }

        [JsonProperty("resourcesFolder")]
        public ResourcesFolder ResourcesFolder { get; set; }


        [JsonProperty("resourcesFolderUrl")]
        public string ResourcesFolderUrl { get; set; }

        public List<EducationSubmissionResource> Resources { get; set; }
    }

    public class SubmittedBy
    {
        [JsonProperty("user")]
        public User User { get; set; }
    }

    public class User
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Username { get; set; }
    }

    public class EducationSubmissionResource
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resource")]
        public EducationResource Resource { get; set; }
    }
}
