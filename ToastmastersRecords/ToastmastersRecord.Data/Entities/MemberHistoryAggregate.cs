using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToastmastersRecord.Data.Entities {
    public class MemberHistoryAggregate {
        [Key]
        public virtual Guid Id { get; set; }
        public virtual string DisplayName { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]        
        public virtual DateTime SpeechCountConfirmedDate { get; set; }
        public virtual int ConfirmedSpeechCount { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime AggregateCalculationDate { get; set; }
        public virtual int CalculatedSpeechCount { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateAsToastmaster { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateAsGeneralEvaluator { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateAsTableTopicsMaster { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateOfLastSpeech { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateOfLastEvaluation { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateOfLastMinorRole { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateOfLastMajorRole { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateOfLastFunctionaryRole { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateOfLastFacilitatorRole { get; set; }
        [DataType(DataType.Date), Column(TypeName = "Date")]
        public virtual DateTime DateOfLastRole { get; set; }
        public virtual bool WillAttend { get; set; }
        public virtual string SpecialRequest { get; set; }
        public virtual int EligibilityCount { get; set; }
    }
}
