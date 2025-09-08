using App.DTO;
using Dom.Repo;
using Domin;
using FluentValidation;
using Infra.Persist;
using Infra.RepositoryImpl;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Books
{
    public static class BooksAccMrk {   }

    public class AddBookCommandHandler : IRequestHandler<AddBookCommand, BookDTO>
    {
        private readonly IGenericRepository<Book> _bookRepository;

        public AddBookCommandHandler(IGenericRepository<Book> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<BookDTO> Handle(AddBookCommand request, CancellationToken cancellationToken)
        {
            var r = request.Book.Adapt<Book>();
            await _bookRepository.AddAsync(r);
            await _bookRepository.SaveChangesAsync();
            return r.Adapt<BookDTO>();
        }
    }

    public class ModifyBookCommandHandler : IRequestHandler<ModifyBookCommand, BookDTO>
    {
        private readonly IGenericRepository<Book> _bookRepository;
        private readonly IGenericRepository<BookCat> _catRepository;

        public ModifyBookCommandHandler(IGenericRepository<Book> bookRepository, IGenericRepository<BookCat> catRepository)
        {
            _bookRepository= bookRepository;
            _catRepository= catRepository;
        }

        public async Task<BookDTO> Handle(ModifyBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetByIdAsync(request.Book.Gid);
            if (book == null)
                throw new Exception("Book not found");

            if (book.CategoryId != request.Book.CategoryId)
            {
                BookCat? cat = await _catRepository.GetByIdAsync(request.Book.CategoryId);
                if (cat == null) throw new Exception("Category not found");
                book.CategoryId = request.Book.CategoryId;
                book.Category = cat;
            }

            Book bc2 = request.Book.Adapt<Book>();

            book.Title = bc2.Title;
            book.Description = bc2.Description;
            book.Authors = bc2.Authors;
            book.Translators = bc2.Translators;

            await _bookRepository.SaveChangesAsync();
            return book.Adapt<BookDTO>();
        }
    }

    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, List<BookDTO>>
    {
        private readonly IGenericRepository<Book> _bookRepository;

        public GetBooksQueryHandler(IGenericRepository<Book> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<List<BookDTO>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            var query = await _bookRepository.GetAllAsync();

            if (request.Id != null)
            {
                query = query.Where(b => b.Gid == request.Id);
            }
            else if (request.Category != null)
            {
                query = query.Where(b => b.CategoryId == request.Category);
            }
            else if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(
                    b =>
                    b.Title.Contains(request.Search) ||
                    b.Authors.Any(Person => Person.Name.Contains(request.Search)) ||
                    b.Translators.Any(Person => Person.Name.Contains(request.Search))
                );
            }

            if (request.page != null && request.pagesize != null)
            {
                int offset = Math.Max((request.page ?? 0) - 1, 0) * (request.pagesize ?? 0);
                query = query.Skip(offset).Take(request.pagesize ?? int.MaxValue - 1);
            }

            return await query.ProjectToType<BookDTO>().ToListAsync(cancellationToken);
        }
    }

    public class AddBookImageCommandHandler : IRequestHandler<AddBookImageCommand, BookPicDTO>
    {
        private readonly BoooooooooksDBContext _context;
        private readonly IGenericRepository<BookPic> _bookPicRepository;
        private readonly IGenericRepository<Book> _bookRepository;

        public AddBookImageCommandHandler(BoooooooooksDBContext context, IGenericRepository<BookPic> bookPicRepository, IGenericRepository<Book> bookRepository)
        {
            _context = context;
            _bookPicRepository = bookPicRepository;
            _bookRepository = bookRepository;
        }

        public async Task<BookPicDTO> Handle(AddBookImageCommand request, CancellationToken cancellationToken)
        {
            BookPic image;
            using var trx = await _context.Database.BeginTransactionAsync();
            try
            {
                var book = await _bookRepository.GetByIdAsync(request.BookId ?? Guid.Empty);
                if (book == null)
                    throw new Exception("Book not found");

                using var ms = new MemoryStream();
                await request.File.CopyToAsync(ms);
                image = new BookPic
                {
                    Content = ms.ToArray()
                };
                await _bookPicRepository.AddAsync(image);
                book.Pics.Add(image.Gid.ToString());
                await _context.SaveChangesAsync(cancellationToken);
                await trx.CommitAsync(cancellationToken);
            }
            catch 
            {
                await trx.RollbackAsync(); throw;
            }
            return image.Adapt<BookPicDTO>();
        }
    }

    public class RemoveBookImageCommandHandler : IRequestHandler<RemoveBookImageCommand, Unit>
    {
        private readonly BoooooooooksDBContext _context;
        private readonly IGenericRepository<BookPic> _bookPicRepository;
        private readonly IGenericRepository<Book> _bookRepository;

        public RemoveBookImageCommandHandler(BoooooooooksDBContext context, IGenericRepository<BookPic> bookPicRepository, IGenericRepository<Book> bookRepository)
        {
            _context = context;
            _bookPicRepository = bookPicRepository;
            _bookRepository = bookRepository;
        }

        public async Task<Unit> Handle(RemoveBookImageCommand request, CancellationToken cancellationToken)
        {
            using var trx = await _context.Database.BeginTransactionAsync();
            try {
                var image = await _bookPicRepository.GetByIdAsync(request.ImageId);
                var book = await _bookRepository.GetByIdAsync(request.BookId ?? Guid.Empty);

                if (image == null || (book != null && book.Pics.Any(s => s == request.ImageId.ToString())))
                    throw new Exception("Image not found");

                book?.Pics.Remove(image.Gid.ToString());
                _bookPicRepository.Delete(image);
                await _context.SaveChangesAsync(cancellationToken);
                await trx.CommitAsync(cancellationToken);
            } catch
            {
                await trx.RollbackAsync();      throw;
            }

            return Unit.Value;
        }

    }

    public class GetBookImageCommandHandler : IRequestHandler<GetBookImageCommand, byte[]>
    {
        private readonly IGenericRepository<BookPic> _bookPicRepository;
        public GetBookImageCommandHandler(IGenericRepository<BookPic> bookPicRepository)
        {
            _bookPicRepository = bookPicRepository;
        }

        public async Task<byte[]> Handle(GetBookImageCommand request, CancellationToken cancellationToken)
        {
            var img = await _bookPicRepository.GetByIdAsync(request.ImageId ?? Guid.Empty);
            if (img == null) throw new Exception("image not found!");
            return img.Content;
        }
    }
    public class RemoveBookCommandHandler : IRequestHandler<RemoveBookCommand, Unit>
    {
        private readonly IGenericRepository<Book> _bookRepository;
        public RemoveBookCommandHandler(IGenericRepository<Book> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Unit> Handle(RemoveBookCommand request, CancellationToken cancellationToken)
        {
            var data = await _bookRepository.GetByIdAsync(request.Gid ?? Guid.Empty);
            if (data == null) throw new Exception("Data not found!");
            _bookRepository.Delete(data);
            await _bookRepository.SaveChangesAsync();
            return Unit.Value;
        }
    }


    public record GetBooksQuery(string? Search, Guid? Id, Guid? Category, int? page, int? pagesize) : IRequest<List<BookDTO>>;
    public record AddBookCommand(BookDTO Book) : IRequest<BookDTO>;
    public record ModifyBookCommand(BookDTO Book) : IRequest<BookDTO>;
    public record AddBookImageCommand(Guid? BookId, IFormFile File) : IRequest<BookPicDTO>;
    public record RemoveBookImageCommand(Guid? BookId, Guid ImageId) : IRequest<Unit>;
    public record GetBookImageCommand(Guid? ImageId) : IRequest<byte[]>;
    public record RemoveBookCommand(Guid? Gid) : IRequest<Unit>;


    public class ModifyBookCommandValidator : AbstractValidator<ModifyBookCommand>
    {
        public ModifyBookCommandValidator()
        {
            RuleFor(x => x.Book)
            .NotNull().WithMessage("Empty Data!");

            RuleFor(x => x.Book.Title)
                .NotEmpty().WithMessage("The Title is required!");

            RuleFor(x => x.Book.Description)
                .NotEmpty().WithMessage("The Description is required!")
                .MinimumLength(4).WithMessage("Must inter valid description!");

            RuleFor(x => x.Book.CategoryId)
                .NotNull().WithMessage("The Category is required!")
                .NotEmpty().WithMessage("The Category is required!");
        }
    }

    public class AddBookCommandValidator : AbstractValidator<AddBookCommand>
    {
        public AddBookCommandValidator()
        {
            RuleFor(x => x.Book)
            .NotNull().WithMessage("Empty Data!");

            RuleFor(x => x.Book.Title)
                .NotEmpty().WithMessage("The Title is required!");

            RuleFor(x => x.Book.Description)
                .NotEmpty().WithMessage("The Description is required!")
                .MinimumLength(4).WithMessage("Must inter valid description!");

            RuleFor(x => x.Book.CategoryId)
                .NotNull().WithMessage("The Category is required!")
                .NotEmpty().WithMessage("The Category is required!");
        }
    }
}
