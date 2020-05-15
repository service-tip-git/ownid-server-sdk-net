using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OwnIdSdk.NetCore3.Web.Abstractions;

namespace OwnIdSdk.NetCore3.Web.FlowEntries
{
    public class UserProfileFormContext<T> : IUserProfileContext where T : class
    {
        private readonly Dictionary<string, IList<string>> _fieldErrors;

        internal UserProfileFormContext(string did, string publicKey, T profile)    
        {
            DID = did;
            PublicKey = publicKey;
            Profile = profile;
            _fieldErrors = new Dictionary<string, IList<string>>();
            GeneralErrors = new List<string>();
        }

        public string DID { get; }

        public string PublicKey { get; }

        public T Profile { get; }

        public List<string> GeneralErrors { get; set; }

        public IReadOnlyDictionary<string, IList<string>> FieldErrors => _fieldErrors;

        public bool HasErrors => GeneralErrors.Any() ||
                                 FieldErrors.Any(x => x.Value.Any());

        public void Validate()
        {
            var results = new List<ValidationResult>();
            _fieldErrors.Clear();

            if (Validator.TryValidateObject(Profile, new ValidationContext(Profile), results, true)) 
                return;
            
            var groupedErrors = results.SelectMany(x =>
                    x.MemberNames.Select(m => (fieldName: m, message: x.ErrorMessage)))
                .GroupBy(x => x.fieldName, x => x.message);

            foreach (var groupedError in groupedErrors)
            {
                var messages = groupedError.ToList();
                _fieldErrors.Add(groupedError.Key, messages);
            }
        }

        // TODO: optimize checking with fallback type for each field
        public void SetError<TField>(Expression<Func<T, TField>> exp, string errorText)
        {
            var type = typeof(T);

            if (!(exp.Body is MemberExpression member))
                throw new ArgumentException($"Expression '{exp}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{exp}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expression '{exp}' refers to a property that is not from type {type}.");

            if (!_fieldErrors.ContainsKey(propInfo.Name))
                _fieldErrors.Add(propInfo.Name, new List<string> {errorText});
            else
                _fieldErrors[propInfo.Name].Add(errorText);
        }

        public void SetGeneralError(string error)
        {
            GeneralErrors.Add(error);
        }
    }
}