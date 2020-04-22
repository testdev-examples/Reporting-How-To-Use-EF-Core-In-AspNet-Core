using System;
using System.Collections.Generic;
using System.Linq;
using xrefcoredemo.Data;
using xrefcoredemo.Models;

namespace xrefcoredemo.Services {
    public class MyEnrollmentsReportRepository {
        readonly int studentId;
        readonly IScopedDbContextProvider<SchoolContext> scopedDbContextProvider;

        public MyEnrollmentsReportRepository() {
            // We use this parameterless constructor in the Data Source Wizard only, and not for the actual instantiation of the repository object.
            throw new NotSupportedException();
        }

        public MyEnrollmentsReportRepository(IScopedDbContextProvider<SchoolContext> scopedDbContextProvider, IUserService userService) {
            this.scopedDbContextProvider = scopedDbContextProvider ?? throw new ArgumentNullException(nameof(scopedDbContextProvider));

            // NOTE: the repository ctor is invoked in the context of http request. At this point of execution we have access to context-dependent data, like currentUserId.
            // The repository MUST read and store all the required context-dependent values for later use. E.g. notice that we do not store the IUserService (which is context/scope dependent), but read the value of current user and store it.
            studentId = userService.GetCurrentUserId();
        }

        //// We have a dilemma here.
        //// This ctor signature better conveys the fact that the repository receives the studentId value.
        //// The downside of the approach is that the repository registraction code is more complicated and must be changed whenever the signature changes.
        //// See the Startup.cs / "Alternative way to register the repository"
        //public MyEnrollmentsReportRepository(IScopedDbContextProvider<SchoolContext> scopedDbContextProvider, int studentId) {
        //    this.scopedDbContextProvider = scopedDbContextProvider ?? throw new ArgumentNullException(nameof(scopedDbContextProvider));
        //    this.studentId = studentId;
        //}

        public StudentDetailsModel GetStudentDetails() {
            using(var dbContextScope = scopedDbContextProvider.GetDbContextScope()) {
                var dbContext = dbContextScope.DbContext;
                var student = dbContext.Students.Find(studentId);

                var model = new StudentDetailsModel {
                    StudentID = student.ID,
                    FirstMidName = student.FirstMidName,
                    LastName = student.LastName,
                    EnrollmentDate = student.EnrollmentDate
                };
                return model;
            }
        }

        public IList<EnrollmentDetailsModel> GetEnrollments() {
            using(var dbContextScope = scopedDbContextProvider.GetDbContextScope()) {
                var dbContext = dbContextScope.DbContext;
                var student = dbContext.Students.Find(studentId);

                dbContext.Entry(student).Collection(x => x.Enrollments).Load();
                student.Enrollments.ToList().ForEach(x => dbContext.Entry(x).Reference(c => c.Course).Load());

                var enrollmentModels = student.Enrollments.Select(x =>
                    new EnrollmentDetailsModel {
                        EnrollmentID = x.EnrollmentID,
                        CourseTitle = x.Course.Title,
                        Grade = x.Grade.HasValue ? x.Grade.Value.ToString() : "NO GRADE YET"
                    });

                return enrollmentModels.ToList();
            }
        }
    }
}
