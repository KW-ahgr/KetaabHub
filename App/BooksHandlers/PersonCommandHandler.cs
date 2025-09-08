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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace App.Books
{
    public static class PersonsAccMrk {   }

    public class GetPersonsCommandHandler : IRequestHandler<GetPersonCommand, List<PersonDTO>>
    {
        private readonly IGenericRepository<Person> _personRepository;
        public GetPersonsCommandHandler(IGenericRepository<Person> personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<List<PersonDTO>> Handle(GetPersonCommand request, CancellationToken cancellationToken)
        {
            var query = await _personRepository.GetAllAsync();

            if (request.page != null && request.pagesize != null)
            {
                int offset = Math.Max((request.page ?? 0) - 1, 0) * (request.pagesize ?? 0);
                query = query.Skip(offset).Take(request.pagesize ?? int.MaxValue - 1);
            }

            return await query.ProjectToType<PersonDTO>().ToListAsync(cancellationToken);
        }
    }
    public class AddPersonCommandHandler : IRequestHandler<AddPersonCommand, PersonDTO>
    {
        private readonly IGenericRepository<Person> _personRepository;
        public AddPersonCommandHandler(IGenericRepository<Person> personRepository) 
        { 
            _personRepository = personRepository;
        }

        public async Task<PersonDTO> Handle(AddPersonCommand request, CancellationToken cancellationToken)
        {
            var r = request.Person.Adapt<Person>();
            await _personRepository.AddAsync(r);
            await _personRepository.SaveChangesAsync();
            return r.Adapt<PersonDTO>();
        }
    }
    public class ModifyPersonCommandHandler : IRequestHandler<ModifyPersonCommand, PersonDTO>
    {
        private readonly IGenericRepository<Person> _personRepository;
        public ModifyPersonCommandHandler(IGenericRepository<Person> personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<PersonDTO> Handle(ModifyPersonCommand request, CancellationToken cancellationToken)
        {
            var person = await _personRepository.GetByIdAsync(request.Person.Gid);
            if (person == null)
                throw new Exception("Data not found");

            person.Name = request.Person.Name;
            person.Description = request.Person.Description;

            await _personRepository.SaveChangesAsync();
            return person.Adapt<PersonDTO>();
        }
    }
    public class RemovePersonCommandHandler : IRequestHandler<RemovePersonCommand, Unit>
    {
        private readonly IGenericRepository<Person> _personRepository;
        public RemovePersonCommandHandler(IGenericRepository<Person> personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<Unit> Handle(RemovePersonCommand request, CancellationToken cancellationToken)
        {
            var data = await _personRepository.GetByIdAsync(request.Gid ?? Guid.Empty);
            if (data == null) throw new Exception("Data not found!");
            _personRepository.Delete(data);
            await _personRepository.SaveChangesAsync();
            return Unit.Value;
        }
    }


    public record AddPersonCommand(PersonDTO Person) : IRequest<PersonDTO>;
    public record ModifyPersonCommand(PersonDTO Person) : IRequest<PersonDTO>;
    public record GetPersonCommand(int? page, int? pagesize) : IRequest<List<PersonDTO>>;
    public record RemovePersonCommand(Guid? Gid) : IRequest<Unit>;


    public class ModifyPersionCommandValidator : AbstractValidator<ModifyPersonCommand>
    {
        public ModifyPersionCommandValidator()
        {
            RuleFor(x => x.Person)
            .NotNull().WithMessage("Empty Data!");

            RuleFor(x => x.Person.Name)
                .NotEmpty().WithMessage("The Name is required!");

            RuleFor(x => x.Person.Description)
                .NotEmpty().WithMessage("The Description is required!")
                .MinimumLength(4).WithMessage("Must inter valid description!");
        }
    }

}
