using App.DTO;
using Dom.Repo;
using Domin;
using FluentValidation;
using Infra.Persist;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Books
{
    public static class CommentsAccMrk {   }

    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, CommentDTO>
    {
        private readonly IGenericRepository<Comment> _commentRepository;

        public AddCommentCommandHandler(IGenericRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<CommentDTO> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            request.Comment.CreatedAt = DateTime.UtcNow;
            var r = request.Comment.Adapt<Comment>();
            await _commentRepository.AddAsync(r);
            await _commentRepository.SaveChangesAsync();

            return r.Adapt<CommentDTO>();
        }
    }

    public class ModifyCommentCommandHandler : IRequestHandler<ModifyCommentCommand, CommentDTO>
    {
        private readonly IGenericRepository<Comment> _commentRepository;

        public ModifyCommentCommandHandler(IGenericRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<CommentDTO> Handle(ModifyCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(request.Comment.Gid);
            if (comment == null) throw new Exception("Comment not found!");

            comment.Title = request.Comment.Title;
            comment.MainBody = request.Comment.MainBody;

            await _commentRepository.SaveChangesAsync();
            return comment.Adapt<CommentDTO>();
        }
    }

    public class ModifyStatusCommentCommandHandler : IRequestHandler<ModifyStatusCommentCommand, CommentDTO>
    {
        private readonly IGenericRepository<Comment> _commentRepository;

        public ModifyStatusCommentCommandHandler(IGenericRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<CommentDTO> Handle(ModifyStatusCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _commentRepository.GetByIdAsync(request.Comment.Gid);
            if (comment == null) throw new Exception("Comment not found!");

            comment.CommentStatus = request.Comment.CommentStatus;

            await _commentRepository.SaveChangesAsync();
            return comment.Adapt<CommentDTO>();
        }
    }

    public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, List<CommentDTO>>
    {
        private readonly IGenericRepository<Comment> _commentRepository;

        public GetCommentsQueryHandler(IGenericRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<List<CommentDTO>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
        {
            var query = await _commentRepository.GetAllAsync();
            query = query.Where(c => c.CommentStatus == Status.ALLOWED);
            
            if (request.BookId.HasValue)
                query = query.Where(c => c.BookId == request.BookId.Value);

            if (request.page != null && request.pagesize != null)
            {
                int offset = Math.Max((request.page ?? 0) - 1, 0) * (request.pagesize ?? 0);
                query = query.Skip(offset).Take(request.pagesize ?? int.MaxValue - 1);
            }

            return await query.ProjectToType<CommentDTO>().ToListAsync(cancellationToken);
        }
    }

    public class GetCommentsForAdminQueryHandler : IRequestHandler<GetCommentsForAdminQuery, List<CommentDTO>>
    {
        private readonly IGenericRepository<Comment> _commentRepository;

        public GetCommentsForAdminQueryHandler(IGenericRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<List<CommentDTO>> Handle(GetCommentsForAdminQuery request, CancellationToken cancellationToken)
        {
            var query = await _commentRepository.GetAllAsync();

            if (request.BookId.HasValue)
                query = query.Where(c => c.BookId == request.BookId.Value);

            if (request.page != null && request.pagesize != null)
            {
                int offset = Math.Max((request.page ?? 0) - 1, 0) * (request.pagesize ?? 0);
                query = query.Skip(offset).Take(request.pagesize ?? int.MaxValue - 1);
            }

            return await query.ProjectToType<CommentDTO>().ToListAsync(cancellationToken);
        }
    }

    public class RemoveCommentCommandHandler : IRequestHandler<RemoveCommentCommand, Unit>
    {
        private readonly IGenericRepository<Comment> _commentRepository;
        public RemoveCommentCommandHandler(IGenericRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<Unit> Handle(RemoveCommentCommand request, CancellationToken cancellationToken)
        {
            var data = await _commentRepository.GetByIdAsync(request.Gid ?? Guid.Empty);
            if (data == null) throw new Exception("Data not found!");
            _commentRepository.Delete(data);
            await _commentRepository.SaveChangesAsync();
            return Unit.Value;
        }
    }
    public class RemoveUserCommentCommandHandler : IRequestHandler<RemoveUserCommentCommand, Unit>
    {
        private readonly IGenericRepository<Comment> _commentRepository;
        public RemoveUserCommentCommandHandler(IGenericRepository<Comment> commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<Unit> Handle(RemoveUserCommentCommand request, CancellationToken cancellationToken)
        {
            var data = await _commentRepository.GetByIdAsync(request.Gid ?? Guid.Empty);
            if (data == null) throw new Exception("Data not found!");

            if (request.userId == null || data.UserId != request.userId) 
                   throw new Exception("Invalid!");

            _commentRepository.Delete(data);
            await _commentRepository.SaveChangesAsync();
            return Unit.Value;
        }
    }


    public record GetCommentsQuery(Guid? BookId, int? page, int? pagesize) : IRequest<List<CommentDTO>>;
    public record GetCommentsForAdminQuery(Guid? BookId, int? page, int? pagesize) : IRequest<List<CommentDTO>>;
    public record AddCommentCommand(CommentDTO Comment) : IRequest<CommentDTO>;
    public record ModifyCommentCommand(CommentDTO Comment) : IRequest<CommentDTO>;
    public record ModifyStatusCommentCommand(CommentDTO Comment) : IRequest<CommentDTO>;
    public record RemoveCommentCommand(Guid? Gid) : IRequest<Unit>;
    public record RemoveUserCommentCommand(Guid? Gid, Guid? userId = null) : IRequest<Unit>;


    public class ModifyCommentCommandValidator : AbstractValidator<ModifyCommentCommand>
    {
        public ModifyCommentCommandValidator()
        {
            RuleFor(x => x.Comment)
            .NotNull().WithMessage("Empty Data!");

            RuleFor(x => x.Comment.MainBody)
                .NotEmpty().WithMessage("The Body is required!")
                .MinimumLength(4).WithMessage("Must inter valid description!");

            RuleFor(x => x.Comment.Title)
                .NotEmpty().WithMessage("The Title is required!");

            RuleFor(x => x.Comment.BookId)
            .NotNull().WithMessage("The Book is null!");

            RuleFor(x => x.Comment.UserId)
            .NotNull().WithMessage("The User is null!");
        }
    }

    public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator()
        {
            RuleFor(x => x.Comment)
            .NotNull().WithMessage("Empty Data!");

            RuleFor(x => x.Comment.MainBody)
                .NotEmpty().WithMessage("The Body is required!")
                .MinimumLength(4).WithMessage("Must inter valid description!");

            RuleFor(x => x.Comment.Title)
                .NotEmpty().WithMessage("The Title is required!");

            RuleFor(x => x.Comment.BookId)
            .NotNull().WithMessage("The Book is null!");

            RuleFor(x => x.Comment.UserId)
            .NotNull().WithMessage("The User is null!");
        }
    }

}
