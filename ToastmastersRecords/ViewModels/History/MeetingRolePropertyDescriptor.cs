using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecords.ViewModels.History {
    public class MeetingRolePropertyDescriptor : PropertyDescriptor {
        private DateTime _date;
        public MeetingRolePropertyDescriptor(DateTime date)
            : base(date.ToString("M"), null) {
            _date = date;
        }

        public override object GetValue(object component) {
            var memberHistory = (MemberHistory)component;
            return memberHistory.GetRole(_date);
        }
        public override void SetValue(object component, object value) {
            throw new NotSupportedException("SetValue");
        }
        public override void ResetValue(object component) {
            throw new NotSupportedException("ResetValue");
        }
        public override bool IsReadOnly {
            get { return true; }
        }
        public override bool CanResetValue(object component) {
            return false;
        }
        public override Type PropertyType {
            get { return typeof(string); }
        }
        public override bool ShouldSerializeValue(object component) {
            return false;
        }
        public override Type ComponentType {
            get {
                return typeof(MemberHistory);
            }
        }
    }
}
