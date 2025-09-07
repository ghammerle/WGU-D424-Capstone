using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C424Assessment.DataRepository;
using C424Assessment.Models;
using Plugin.LocalNotification;
using static C424Assessment.Models.Enumerations;

namespace C424Assessment
{
    public static class NotificationHelper
    {
        public static async Task CheckNotifications(IDataRepository repo, int userId)
        {
            Console.WriteLine("Notification Helper starting");
            try
            {
                var mapData = await repo.GetCourseMapDataByUserId(userId);
                var filteredMaps = mapData.Where(md => md.StartDate.AddDays(-7) <= DateTime.Today && DateTime.Today <= md.EndDate).ToList();
                foreach (var md in filteredMaps)
                {
                    if (md.NotificationsEnabled == false ||md.CourseStatusId == (int)CourseStatus.Completed)
                    {
                        continue;
                    }
                    await CheckCourseNotifications(repo, md);
                }
                Console.WriteLine("Notification Helper ending");
            }
            catch (Exception e)
            {
                await OnError($"Checknotifications Failed {e.Message}");
            }
        }

        public static async Task CheckCourseNotifications(IDataRepository repo, CourseMapModel mapdata)
        {
            var course = await repo.GetCourseById(mapdata.CourseId);
            // alert the user if the term ends in 1 month or less and course the need to start
            if ((Enumerations.CourseStatus)mapdata.CourseStatusId == CourseStatus.Planned &&
                    mapdata.EndDate.Date <= DateTime.Today.AddMonths(1))
                {
                var request = await CreateNotification(Guid.NewGuid().GetHashCode(), $"{course.Name} Date Notification", "Term End Date Notification", 
                    $"Term End Date: {mapdata.EndDate.ToShortDateString()} please review {course.Name} , it has not been started.");
                    
                    await LocalNotificationCenter.Current.Show(request);
                }
                // alert the user if the course end date is within 7 days and the course is active
                else if ((Enumerations.CourseStatus)mapdata.CourseStatusId == CourseStatus.Active &&
                    mapdata.EndDate.Date >= DateTime.Today &&
                    mapdata.EndDate.Date <= DateTime.Today.AddDays(7))
                {
                    var request = await CreateNotification(Guid.NewGuid().GetHashCode(), $"{course.Name} Date Notification", "Course End Date Notification", $"Course End Date: {mapdata.EndDate.ToShortDateString()} please complete {course.Name}.");
                    await LocalNotificationCenter.Current.Show(request);
                }
                if ((Enumerations.CourseStatus)mapdata.CourseStatusId == CourseStatus.Active)
                {
                    await CheckAssessmentNotifications(repo, course.CourseId);
                }
         }

        public static async Task<NotificationRequest> CreateNotification(int notificationId, string title, string subtitle, string description)
        {
            var request = new NotificationRequest
            {
                NotificationId = notificationId,
                Title = title,
                Subtitle = subtitle,
                Description = description,
                BadgeNumber = 42,
            };
            if (request is null)
            {
                await OnError("Failed to create notification request.");
                request = new NotificationRequest { NotificationId = -1, Title = "Notification Error", Subtitle = "Error", Description = "Error", BadgeNumber = 0 };
            }
            return request;
        }
        private static async Task CheckAssessmentNotifications(IDataRepository repo,  int courseId)
        {
            // if the related course is not active do not notify
            // if the assessment end (due) date is less than 7 days away or elapsed notify the user
            var assessments = await repo.GetAssessmentsByCourseId(courseId);
            var course = await repo.GetCourseById(courseId);

            foreach (var assessment in assessments)
            {
                if (assessment.NotificationsEnabled == false)
                {
                    continue;
                }
                // if the assessment is due in 7 days or less alert the user
                if (assessment.EndDate >= DateTime.Today &&
                    assessment.EndDate <= DateTime.Today.AddDays(7))
                {
                    var request = await CreateNotification(Guid.NewGuid().GetHashCode(),
                        $"{assessment.Name} Due Date Notification", "Assessment Due Date Notification",
                        $"Assessment Due Date: {assessment.EndDate.ToShortDateString()} please complete {assessment.Name}, for {course.Name}.");
                    await LocalNotificationCenter.Current.Show(request);
                }
                // if the assessment start date is today or later inform the user assessment is available
                else if (assessment.StartDate <= DateTime.Now)
                {
                    var request = await CreateNotification(Guid.NewGuid().GetHashCode(),
                        $"{assessment.Name} Available Notification",
                        "Assessment Available Notification",
                        $"Assessment Available: {assessment.Name} is available now and due on {assessment.EndDate.ToShortDateString()}, for {course.Name}.");
                    await LocalNotificationCenter.Current.Show(request);
                }
            }
        }

        public static async Task OnError(string message)
        {
            await Shell.Current.DisplayAlert("Error", message, "OK");
        }

    }
}
