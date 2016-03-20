using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Unclassified.Util;

namespace Mattermost.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        CollectionDictionary<string, string> DependentNotifications
        {
            get
            {
                if (dependentNotifications == null)
                {
                    dependentNotifications = new CollectionDictionary<string, string>();

                    foreach (PropertyInfo prop in GetType().GetProperties())
                    {
                        foreach (NotifiesOnAttribute attr in prop.GetCustomAttributes<NotifiesOnAttribute>(false))
                        {
                            foreach (string dependent in attr.Names)
                            {
#if DEBUG
                                if (!TypeDescriptor.GetProperties(this).OfType<PropertyDescriptor>().Any(d => d.Name == dependent))
                                {
                                    throw new ArgumentException("Specified property " + GetType().Name + "." + prop.Name + " to notify on non-existing property " + dependent);
                                }
#endif

                                dependentNotifications.Add(dependent, prop.Name);
                            }
                        }
                    }
                }

                return dependentNotifications;
            }
        }
        Dictionary<string, MethodInfo> PropertyValidators
        {
            get
            {
                if (propertyValidators == null)
                {
                    propertyValidators = new Dictionary<string, MethodInfo>();

                    foreach (MethodInfo func in GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod))
                    {
                        ValidatesPropertyAttribute attr = func.GetCustomAttribute<ValidatesPropertyAttribute>(false);

                        if (attr == null)
                            continue;
#if DEBUG
                        IEnumerable<PropertyDescriptor> properties = TypeDescriptor.GetProperties(this).OfType<PropertyDescriptor>();

                        if (!properties.Any(d => d.Name == attr.Property))
                        {
                            throw new ArgumentException(string.Format("Specified function {0}.{1} to validate a non-existing property {2}", GetType().Name, func.Name, attr.Property));
                        }

                        ParameterInfo[] parameters = func.GetParameters();

                        if (parameters.Length != 1)
                            throw new ArgumentException(string.Format("Specified validator function {0}.{1} must only have 1 parameter", GetType().Name, func.Name));

                        PropertyDescriptor property = properties.First(p => p.Name == attr.Property);

                        if (property.PropertyType != parameters[0].ParameterType)
                            throw new ArgumentException(string.Format("Specified validator function {0}.{1} takes parameter of wrong type (expecting {2}, found {3})", GetType().Name, func.Name, property.PropertyType.Name, parameters[0].ParameterType));

                        if (propertyValidators.ContainsKey(attr.Property))
                            throw new ArgumentException(string.Format("Validator for property {0} already exists (only 1 allowed)", attr.Property));
#endif

                        propertyValidators.Add(attr.Property, func);
                    }
                }

                return propertyValidators;
            }
        }

        Dictionary<string, object> backingFields = new Dictionary<string, object>();
        Dictionary<string, MethodInfo> propertyValidators;
        CollectionDictionary<string, string> dependentNotifications;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
#if DEBUG
            if (!TypeDescriptor.GetProperties(this).OfType<PropertyDescriptor>().Any(d => d.Name == property))
            {
                throw new ArgumentException("Notifying a change of non-existing property " + GetType().Name + "." + property);
            }
#endif

            if (PropertyChanged != null)
                InvokeOnMainThread(() => PropertyChanged(this, new PropertyChangedEventArgs(property)));

            foreach (string dependentProperty in DependentNotifications.GetValuesOrEmpty(property))
                OnPropertyChanged(dependentProperty);
        }

        public void InvokeOnMainThread(Action action)
        {
            Dispatcher dispatcher = Application.Current.Dispatcher;

            if (dispatcher == null || dispatcher.Thread == Thread.CurrentThread)
                action();
            else
                dispatcher.Invoke(action);
        }

        public T GetValue<T>([CallerMemberName] string property = "")
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException("property");

            object value;

            if (backingFields.TryGetValue(property, out value))
                return (T)value;

            return default(T);
        }

        public bool SetValue<T>(T newValue, [CallerMemberName] string property = "")
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException("property");

            if (backingFields.ContainsKey(property) && EqualityComparer<T>.Default.Equals(newValue, GetValue<T>(property)))
                return false;

            if (PropertyValidators.ContainsKey(property))
            {
                bool validated = (bool)PropertyValidators[property].Invoke(this, new object[] { newValue });

                if (!validated)
                    return false;
            }

            backingFields[property] = newValue;
            OnPropertyChanged(property);

            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    class NotifiesOnAttribute : Attribute
    {
        public List<string> Names { get; private set; }
        public override object TypeId
        {
            get { return this; }
        }

        public NotifiesOnAttribute(params string[] properties)
        {
            if (properties == null || properties.Length == 0)
                throw new ArgumentNullException("properties");

            Names = new List<string>();

            foreach (string property in properties)
            {
                if (string.IsNullOrEmpty(property))
                    throw new ArgumentException("One or more property names are null or empty strings");

                Names.Add(property);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    class ValidatesPropertyAttribute : Attribute
    {
        public string Property { get; private set; }
        public override object TypeId
        {
            get { return this; }
        }

        public ValidatesPropertyAttribute(string property)
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException("property");

            Property = property;
        }
    }
}
