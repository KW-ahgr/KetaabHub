using App.Books;
using App.DTO;
using Domin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Api.Controllers
{
    /// <summary>
    /// Main point (controller)
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BookController : Controller
    {
        private readonly IMediator _mediator;

        public BookController(IMediator mediator) : base()
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrives All BookCategories with optional arguments
        /// </summary>
        /// <param name="gd"></param>
        /// <param name="search"></param>
        /// <returns>
        /// A Collection of BookCateguries
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Cats(Guid? gd = null, string? search = null) /////
        {
            var res = await _mediator.Send(new GetCategoryQuery(gd, search));
            return Ok(res);
        }

        /// <summary>
        /// Retrives All Books with optional arguments
        /// </summary>
        /// <param name="search"></param>
        /// <param name="id"></param>
        /// <param name="cat"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns>
        /// A Collection of Books
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Books(string? search = null, Guid? id = null, Guid? cat = null, int? page = null, int? pagesize = null) /////
        {
            var res = await _mediator.Send(new GetBooksQuery(search, id, cat, page, pagesize));
            return Ok(res);
        }

        /// <summary>
        /// Inserts new comment for a book
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>
        /// Same comments was saved
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CommentDTO comment) /////
        {
            var res = await _mediator.Send(new AddCommentCommand(comment));
            return Ok(res);
        }

        /// <summary>
        /// Retrives All Comments with optional arguments for users
        /// </summary>
        /// <param name="book"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns>
        /// A Collection of Comments
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Comments(Guid? book = null, int? page = null, int? pagesize = null) /////
        { 
            var res = await _mediator.Send(new GetCommentsQuery(book, page, pagesize));
            return Ok(res);
        }

        /// <summary>
        /// Retrives All Comments with optional arguments just for admin (containes all of comment-statuses)
        /// </summary>
        /// <param name="book"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns>
        /// A Collection of Comments
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CommentsForAdmin(Guid? book = null, int? page = null, int? pagesize = null) /////
        {
            var res = await _mediator.Send(new GetCommentsForAdminQuery(book, page, pagesize));
            return Ok(res);
        }

        /// <summary>
        /// Modifies main properties of a Comment
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>
        /// Same Comment was updated
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> ModifyCommnent([FromBody] CommentDTO comment)
        {
            var res = await _mediator.Send(new ModifyCommentCommand(comment));
            return Ok(res);
        }

        /// <summary>
        /// Retrives an Image by id
        /// </summary>
        /// <param name="image"></param>
        /// <returns>
        /// an image if matches
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetImage(Guid? image = null) /////
        {
            var res = await _mediator.Send(new GetBookImageCommand(image));
            return File(res, $"_{image}.jpg");
        }

        /// <summary>
        /// Retrives All Persons(Translators and Writers) with optional arguments
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns>
        /// A Collection of Persons
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetPersons(int? page = null, int? pagesize = null) 
        {
            var res = await _mediator.Send(new GetPersonCommand(page, pagesize));
            return Ok(res);
        }

        /// <summary>
        /// Deletes A Comment
        /// </summary>
        /// <param name="Id"></param>
        [HttpDelete]
        public async Task<IActionResult> DeleteUserComment(Guid Id) /////
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var res = await _mediator.Send(new RemoveUserCommentCommand(Id, Guid.Parse(userId)));
            return Ok(res);
        }

        /// <summary>
        /// Changes the status of a comment by admin - for example the admin reads a comment and allow it to show to other users
        /// </summary>
        /// <param name="commnent"></param>
        /// <returns>
        /// same comment was changed
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ModifyCommnentStatus([FromBody] CommentDTO commnent)
        {
            var res = await _mediator.Send(new ModifyCommentCommand(commnent));
            return Ok(res);
        }

        /// <summary>
        /// Adds new Book by admin
        /// </summary>
        /// <param name="book">BookDTO</param>
        /// <returns>
        /// same Book was Added
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBook([FromBody] BookDTO book) /////
        {
            var res = await _mediator.Send(new AddBookCommand(book));
            return Ok(res);
        }

        /// <summary>
        /// Adds new Person by admin
        /// </summary>
        /// <param name="person">PersonDTO</param>
        /// <returns>
        /// same person was added
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPerson([FromBody] PersonDTO person) /////
        {
            var res = await _mediator.Send(new AddPersonCommand(person));
            return Ok(res);
        }

        /// <summary>
        /// Adds new BookCategury by admin
        /// </summary>
        /// <param name="cat">BookCatDTO</param>
        /// <returns>
        /// same Categury was added
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCat([FromBody] BookCatDTO cat) /////
        {
            var res = await _mediator.Send(new AddCategoryQuery(cat));
            return Ok(res);
        }

        /// <summary>
        /// Modifies BookCategury by admin
        /// </summary>
        /// <param name="cat">BookCatDTO</param>
        /// <returns>
        /// same categury was modified
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ModifyCat([FromBody] BookCatDTO cat) /////
        {
            var res = await _mediator.Send(new ModifyCategoryQuery(cat));
            return Ok(res);
        }

        /// <summary>
        /// Modifies a Book by admin
        /// </summary>
        /// <param name="book">BookDTO</param>
        /// <returns>
        /// same Book was modified
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ModifyBook([FromBody] BookDTO book) /////
        {
            var res = await _mediator.Send(new ModifyBookCommand(book));
            return Ok(res);
        }

        /// <summary>
        /// Modifies a Person by admin
        /// </summary>
        /// <param name="person">PersonDTO</param>
        /// <returns>
        /// returns same Person was modified
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ModifyPerson([FromBody] PersonDTO person) /////
        {
            var res = await _mediator.Send(new ModifyPersonCommand(person));
            return Ok(res);
        }

        /// <summary>
        /// Adds an image to a book by admin
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="file"></param>
        /// <returns>
        /// same bookpic was added to book
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPicToBook(Guid bookId, IFormFile file) /////
        {
            var res = await _mediator.Send(new AddBookImageCommand(bookId, file));
            return Ok(res);
        }

        /// <summary>
        /// Removes an image from book by admin
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="pic"></param>
        /// <returns>
        /// same image wes removed from book
        /// </returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemovePictureFromBook(Guid bookId, Guid pic) /////
        {
            var res = await _mediator.Send(new RemoveBookImageCommand(bookId, pic));
            return Ok(res);
        }

        /// <summary>
        /// Deletes a Book by admin
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete] 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(Guid Id) /////
        {
            var res = await _mediator.Send(new RemoveBookCommand(Id));
            return Ok(res);
        }

        /// <summary>
        /// Deletes a person by admin
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>
        /// same Person was deletted
        /// </returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePerson(Guid Id) /////
        {
            var res = await _mediator.Send(new RemovePersonCommand(Id));
            return Ok(res);
        }

        /// <summary>
        /// Deletes a Comment by admin
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>
        /// same Comment was deletted
        /// </returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteComment(Guid Id) /////
        {
            var res = await _mediator.Send(new RemoveCommentCommand(Id));
            return Ok(res);
        }

        /// <summary>
        /// Deletes a BookCategury by admin
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>
        /// same BookCategury was deletted
        /// </returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCat(Guid Id) /////
        {
            var res = await _mediator.Send(new RemoveCategoryQuery(Id));
            return Ok(res);
        }

    }

}
