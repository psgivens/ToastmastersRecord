using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecords.ViewModels.History {
    public class MemberHistoryDescriptionProvider : TypeDescriptionProvider {
        private readonly static MemberHistoryDescriptionProvider _instance = new MemberHistoryDescriptionProvider();
        private DateTime[] _dates;
        static MemberHistoryDescriptionProvider() {
            TypeDescriptor.AddProvider(_instance, typeof(MemberHistory));
        }

        /// <summary>
        /// An initialization method which forces the static constructor to execute. 
        /// </summary>
        public static void Register(params DateTime[] dates) {
            // Forces the static constructor to execute the first time this is called. 
            _instance._dates = dates;
        }

        public MemberHistoryDescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(MemberHistory))) { }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
            return new MemberHistoryDescriptor(base.GetTypeDescriptor(objectType, instance), _dates);
        }
    }
}
