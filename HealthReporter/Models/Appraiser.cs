using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthReporter.Models
{
    public class Appraiser : INotifyPropertyChanged, IDataErrorInfo
    {
        public byte[] id { get; set; }
        
        public string updated { get; set; }

        private string _name;



        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("name");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDataErrorInfo Members
        string IDataErrorInfo.Error
        {
            get
            {
                return null;
            }
        }



        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }


        #endregion

        #region Validation

        static readonly string[] ValidatedProperties =
       {
            "name"
        };

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                {
                    if (GetValidationError(property) != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "name":
                    error = ValidateName();
                    break;
               
            }
            return error;
        }

        private string ValidateName()
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return "Appraiser's name can not be empty.";
            }
            else
            {
                return null;
            }
        }
      
        #endregion

    }
}
