using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ToastmastersRecords.Data;

namespace ToastmastersRecords.Controls {
    public class RoleTypeToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture) {
            var roleType = value as MeetingRoleType;
            return roleType != null && targetType == typeof(string) 
                ? roleType.Role
                : value;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
            return value;
        }
    }
}
