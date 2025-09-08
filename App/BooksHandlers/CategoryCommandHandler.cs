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
    public static class CatsAccMrk {   }

    public class GetCategoryCommandHandler : IRequestHandler<GetCategoryQuery, List<BookCatDTO>>
    {
        private readonly IGenericRepository<BookCat> _catRepository;

        public GetCategoryCommandHandler(IGenericRepository<BookCat> catRepository)
        {
            _catRepository = catRepository;
        }

        public async Task<List<BookCatDTO>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            var query = await _catRepository.GetAllAsync();
            if (request.Gd != null) 
                query = query.Where(c => c.Gid == request.Gd);
            else if (!string.IsNullOrEmpty(request.Search)) 
                query = query.Where(c => c.Name.Contains(request.Search));
            return await query.ProjectToType<BookCatDTO>().ToListAsync(cancellationToken);
        }

    }

    public class AddCateguryCommandHandler : IRequestHandler<AddCategoryQuery, BookCatDTO>
    {
        private readonly IGenericRepository<BookCat> _catRepository;
        public AddCateguryCommandHandler(IGenericRepository<BookCat> catRepository)
        {
            _catRepository = catRepository;
        }
        public async Task<BookCatDTO> Handle(AddCategoryQuery request, CancellationToken cancellationToken)
        {
            var r = request.Cat.Adapt<BookCat>();
            await _catRepository.AddAsync(r);
            await _catRepository.SaveChangesAsync();
            return r.Adapt<BookCatDTO>();
        }
    }

    public class ModifyCateguryCommandHandler : IRequestHandler<ModifyCategoryQuery, BookCatDTO>
    {
        private readonly IGenericRepository<BookCat> _catRepository;
        public ModifyCateguryCommandHandler(IGenericRepository<BookCat> catRepository)
        {
            _catRepository = catRepository;
        }
        public async Task<BookCatDTO> Handle(ModifyCategoryQuery request, CancellationToken cancellationToken)
        {
            var ct = await _catRepository.GetByIdAsync(request.Cat.Gid);
            if (ct == null) throw new Exception("Category not found");

            ct.Name = request.Cat.Name;
            ct.Description = request.Cat.Description;

            await _catRepository.SaveChangesAsync();
            return ct.Adapt<BookCatDTO>();
        }
    }
    public class RemoveCategoryCommandHandler : IRequestHandler<RemoveCategoryQuery, Unit>
    {
        private readonly IGenericRepository<BookCat> _catRepository;
        public RemoveCategoryCommandHandler(IGenericRepository<BookCat> catRepository)
        {
            _catRepository = catRepository;
        }

        public async Task<Unit> Handle(RemoveCategoryQuery request, CancellationToken cancellationToken)
        {
            var data = await _catRepository.GetByIdAsync(request.Gid ?? Guid.Empty);
            if (data == null) throw new Exception("Data not found!");
            _catRepository.Delete(data);
            await _catRepository.SaveChangesAsync();
            return Unit.Value;
        }
    }


    public record GetCategoryQuery(Guid? Gd, string? Search) : IRequest<List<BookCatDTO>>;
    public record AddCategoryQuery(BookCatDTO Cat) : IRequest<BookCatDTO>;
    public record ModifyCategoryQuery(BookCatDTO Cat) : IRequest<BookCatDTO>;
    public record RemoveCategoryQuery(Guid? Gid) : IRequest<Unit>;


    public class AddCatCommandValidator : AbstractValidator<AddCategoryQuery>
    {
        public AddCatCommandValidator()
        {
            RuleFor(x => x.Cat)
            .NotNull().WithMessage("Empty Data!");

            RuleFor(x => x.Cat.Name)
                .NotEmpty().WithMessage("The Name is required!");

            RuleFor(x => x.Cat.Description)
                .NotEmpty().WithMessage("The Description is required!")
                .MinimumLength(4).WithMessage("Must inter valid description!");
        }
    }
    public class ModifyCatCommandValidator : AbstractValidator<ModifyCategoryQuery>
    {
        public ModifyCatCommandValidator()
        {
            RuleFor(x => x.Cat)
            .NotNull().WithMessage("Empty Data!");

            RuleFor(x => x.Cat.Name)
                .NotEmpty().WithMessage("The Name is required!");

            RuleFor(x => x.Cat.Description)
                .NotEmpty().WithMessage("The Description is required!")
                .MinimumLength(4).WithMessage("Must inter valid description!");
        }
    }

}
