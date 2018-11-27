using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecords.ViewModels.History {
    public class MemberHistoryDescriptor : CustomTypeDescriptor {
        private readonly MeetingRolePropertyDescriptor[] _roleDescriptors;
        internal MemberHistoryDescriptor(ICustomTypeDescriptor parent, params DateTime[] meetingDates) : base(parent) {
            _roleDescriptors = new MeetingRolePropertyDescriptor[meetingDates.Length];
            for(int i = 0; i < meetingDates.Length; i++) {
                _roleDescriptors[i] = new MeetingRolePropertyDescriptor(meetingDates[i]);
            }
        }
        public override PropertyDescriptorCollection GetProperties() {
            var properties = base.GetProperties();
            var descriptors = new List<PropertyDescriptor>(properties.OfType<PropertyDescriptor>());
            descriptors.AddRange(_roleDescriptors);
            properties = new PropertyDescriptorCollection(descriptors.ToArray());
            return properties;
        }
    }
}
