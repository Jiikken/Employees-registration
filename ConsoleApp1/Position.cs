using System.Collections.Generic;
using System.Linq;

namespace PersonStructure
{
    public class Position
    {
        private readonly static Dictionary<string, Position> _positions = new Dictionary<string, Position>();

        private readonly List<Person> _persons = new List<Person>();
        private readonly string _title;

        public Position(string title)
        {
            _title = title;
        }

        public static bool IsPositionExist(string position)
        {
            return _positions.ContainsKey(position);
        }

        public static Position GetPositionByTitle(string position)
        {
            return _positions[position];
        }
        public static Position CreatePosition(string position)
        {
            Position createdPosition = new Position(position);
            _positions.Add(position, createdPosition);
            return createdPosition;
        }

        public static List<Position> GetPositions()
        {
            return _positions.Values.ToList();
        }

        public string GetTitle()
        {
            return _title;
        }

        public void AddPerson(Person person)
        {
            if (IsPersonExist(person))
            {
                return;
            }
            _persons.Add(person);
        }

        public bool IsPersonExist(Person person)
        {
            return _persons.Contains(person);
        }

        public void RemovePerson(Person person)
        {
            if (!IsPersonExist(person))
            {
                return;
            }
            _persons.Remove(person);
        }

        public List<Person> GetPersons()
        {
            return _persons;
        }

        public void RemovePositionIfNoPersons()
        {
            if (!IsPositionExist(_title))
            {
                return;
            }
            if (_persons.Count == 1)
            {
                _positions.Remove(_title);
            }
        }
    }
}
