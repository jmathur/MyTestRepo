using Aerospike.Client;
using StudentApp.Models;

namespace StudentApp.Services
{
    public class AerospikeService : IDisposable
    {
        private readonly AerospikeClient _client;
        private readonly string _namespace = "test";
        private readonly string _setName = "students";

        public AerospikeService()
        {
            _client = new AerospikeClient("127.0.0.1", 3000);
        }

        public void Create(Student student)
        {
            if (string.IsNullOrEmpty(student.Id))
            {
                student.Id = Guid.NewGuid().ToString();
            }

            Key key = new Key(_namespace, _setName, student.Id);
            
            Bin binId = new Bin("Id", student.Id);
            Bin binName = new Bin("Name", student.Name ?? string.Empty);
            Bin binAge = new Bin("Age", student.Age);
            Bin binGrade = new Bin("Grade", student.Grade ?? string.Empty);

            _client.Put(null, key, binId, binName, binAge, binGrade);
        }

        public Student? Read(string id)
        {
            Key key = new Key(_namespace, _setName, id);
            Record record = _client.Get(null, key);

            if (record == null)
            {
                return null;
            }

            return new Student
            {
                Id = record.GetString("Id") ?? id,
                Name = record.GetString("Name") ?? string.Empty,
                Age = record.GetInt("Age"),
                Grade = record.GetString("Grade") ?? string.Empty
            };
        }

        public void Update(Student student)
        {
            Create(student);
        }

        public void Delete(string id)
        {
            Key key = new Key(_namespace, _setName, id);
            _client.Delete(null, key);
        }

        public List<Student> GetAll()
        {
            List<Student> students = new List<Student>();
            
            _client.ScanAll(null, _namespace, _setName, (key, record) =>
            {
                students.Add(new Student
                {
                    Id = record.GetString("Id") ?? string.Empty,
                    Name = record.GetString("Name") ?? string.Empty,
                    Age = record.GetInt("Age"),
                    Grade = record.GetString("Grade") ?? string.Empty
                });
            });

            return students;
        }

        public void Dispose()
        {
            _client?.Close();
        }
    }
}
