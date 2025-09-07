using C424Assessment.DataRepository;
using C424Assessment.Models;

namespace C424Assessment.Views;

[QueryProperty(nameof(TermId), "TermId")]
[QueryProperty(nameof(CourseId), "CourseId")]
[QueryProperty(nameof(NoteId), "NoteId")]
public partial class NotesPage : BaseContentPage
{
    public string CourseId { get; set; } = string.Empty;
    public string NoteId { get; set; } = string.Empty;
    public string TermId { get; set; } = string.Empty;
    public NotesModel? Note { get; set; }
    public NotesPage(IDataRepository SQLiteRepository) : base(SQLiteRepository)
    {
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        if (string.IsNullOrEmpty(CourseId))
        {
            // this should not happen and is a programming error
            await OnError("There is not a course to link the note to.");
        }
        var course = await DataRepository.GetCourseById(int.Parse(CourseId));
        this.Title = $"Notes for {course.Name}";
        if (!string.IsNullOrEmpty(NoteId))
        {
            Note = await DataRepository.GetNoteById(int.Parse(NoteId));
            if (Note != null)
            {
                // Populate the UI with the existing note data
                NotesEditor.Text = Note.Content;
            }
            else
            {
                // If no note exists, create a new one
                Note = new NotesModel();
                NotesEditor.Text = string.Empty;
            }
        }
    }

    private async void FinishButtons_OnCancel(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={CourseId}&ActiveUserId={ActiveUserId}&TermId={TermId} ");
    }

    private async void FinishButtons_OnDelete(object sender, EventArgs e)
    {
        if (Note is null)
        {
            throw new InvalidOperationException("Note should not be null here.");
        }
        await DataRepository.DeleteNote(Note.Id);
        await Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={CourseId}&ActiveUserId={ActiveUserId}&TermId={TermId} ");
    }

    private async void FinishButtons_OnSave(object sender, EventArgs e)
    {
        var timeStamp = DateTime.Now;

        if (Note is null)
        {
            // create a new Note
            var newNote = new NotesModel { Content = $"{NotesEditor.Text}", LastEdited = timeStamp };
            await DataRepository.AddOrUpdateNote(newNote);
            // get not just added so the _courseMapModel can be updated
            Note = await DataRepository.GetLatestNote();
            if (Note is null || Note.LastEdited != timeStamp)
            {
                await OnError("Failed to save note.");
                return;
            }
            // update coursemapmodel with the new note
            var courseMapModels = await DataRepository.GetCourseMaps();
            var courseMapModel = courseMapModels.FirstOrDefault(c => c.CourseId == int.Parse(CourseId) && c.UserId == ActiveUserId);
            if (courseMapModel is null)
            {
                await OnError("Failed to find course map for user.");
                return;
            }
            courseMapModel.NotesId = Note.Id;
            await DataRepository.AddOrUpdateCourseMap(courseMapModel);
           
        }
        var note = new NotesModel { Id = Note.Id, Content = $"{NotesEditor.Text}", LastEdited = timeStamp };
        await  DataRepository.AddOrUpdateNote(note);

        await LogEvent($"Note saved with Id {Note.Id}");
        await Shell.Current.GoToAsync($"{nameof(CoursePage)}?CourseId={CourseId}&ActiveUserId={ActiveUserId}&TermId={TermId} ");
    }

    private async void ShareButton_Clicked(object sender, EventArgs e)
    {
        await ShareText(NotesEditor.Text);
    }
    public async Task ShareText(string text)
    {
        var course = await DataRepository.GetCourseById(int.Parse(CourseId));
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = text,
            Title = $"{course.Name} Notes"
        });
    }
}