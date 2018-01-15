using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quartz;

namespace LeBlancCodes.Integrations.Jobs
{
    public abstract class BaseJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var data = context.MergedJobDataMap;

            //throw new NotImplementedException();
        }

        protected abstract Task ExecuteAsync(IJobExecutionContext context);

        private void ApplyJobData(JobDataMap mergedJobData)
        {
            
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JobDataAttribute : Attribute
    {
        internal object GetValue(PropertyInfo property, JobDataMap mergedJobData)
        {
            if (string.IsNullOrWhiteSpace(Key))
                Key = property.Name;

            var data = mergedJobData.GetString(Key);
            if (string.IsNullOrWhiteSpace(data))
                return property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

            var serializer = JsonSerializer.CreateDefault();
            using (var reader = new JsonTextReader(new StringReader(data)))
                return serializer.Deserialize(reader, property.PropertyType);
        }

        public JobDataAttribute()
        {
        }

        public JobDataAttribute(string key) => Key = key;

        public string Key { get; set; }
    }
}
