using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C424Assessment.Models
{
    public class Enumerations
    {
        public enum AssementType
        {
            Performance,
            Objective
        }

        public enum CourseStatus
        {
            Planned,
            Active,
            Completed
        }

        public enum AssessementStatus
        {
            NotStarted,
            InProgress,
            Completed
        }
        public enum UserRole
        {
            None,
            Administrator,
            Student
        }
    }
}
