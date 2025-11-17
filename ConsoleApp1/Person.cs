namespace PersonStructure
{
    public class Person
    {
        private const int _persons = 4;

        private readonly string _firstName;
        private readonly string _lastName;
        private Position _position = null;
        private double _salary = 0.0D;

        public Person(string fullname)
        {
            string[] parts = fullname.Split(' ');
            _firstName = parts[0];
            _lastName = parts[1];
        }

        public string GetFirstname()
        {
            return _firstName;
        }

        public string GetLastname()
        {
            return _lastName;
        }

        public Position GetPosition()
        {
            return _position;
        }

        public void SetPosition(Position position)
        {
            _position?.RemovePerson(this);
            _position = position;
            position?.AddPerson(this);

        }

        public double GetSalary()
        {
            return _salary;
        }
        public void SetSalary(double salary)
        {
            _salary = salary;
        }

        public static int GetCountPersons()
        {
            return _persons;
        }
    }

    public class InfoPersonXML
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Title { get; set; }
        public double Salary { get; set; }
        public InfoPersonXML() { }
        public InfoPersonXML(string firstname, string lastname, string title, double salary)
        {
            Firstname = firstname; Lastname = lastname; Title = title; Salary = salary;
        }
    }
}
