using Dom.Repo;
using Domin;
using Infra.Persist;
using Infra.RepositoryImpl;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tst;

[TestFixture]
public class Tests
{

    private BoooooooooksDBContext _context;
    private IGenericRepository<Book> _bookRepository;
    private IGenericRepository<BookCat> _catRepository;
    private IGenericRepository<Person> _personRepository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BoooooooooksDBContext>()
                .UseInMemoryDatabase(databaseName : "test01")
                .Options;
        _context = new BoooooooooksDBContext(options);

        _bookRepository = new BookRepository(_context);
        _catRepository = new BookCatRepository(_context);
        _personRepository = new PersonRepository(_context);
    }

    [Test]
    public async Task AddBook()
    {
        Book book = new Book()
        {
            Title = "Test",
            Description = "Luromipsun",
        };
        TestSaving(book, _bookRepository);
    }

    [Test]
    public async Task AddCategory()
    {
        BookCat cat = new BookCat()
        {
            Name = "Test",
            Description = "Luromipsun"
        };
        TestSaving(cat, _catRepository);
    }

    [Test]
    public async Task AddPerson()
    {
        Person person = new Person()
        {
            Name = "Test",
            Description = "Luromipson"
        };
        TestSaving(person, _personRepository);
    }

    [TearDown]
    public void Dispose()
    { 
        _context.Dispose();
    }

    private async void TestSaving<T> (T obj, IGenericRepository<T> repository)
        where T : class
    {
        await repository.AddAsync(obj);
        await repository.SaveChangesAsync();
        var r = await repository.GetAllAsync();
        Assert.That(r.ToArray().Length, Is.GreaterThan(0));
    }
}
