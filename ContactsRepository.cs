using HelloWorldService.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApi
{
    public interface IContactRepository
    {
        IEnumerable<Contact> Contacts { get; }

        void Add(Contact contact);
        bool Update(int contactId, Contact contact);
        bool Delete(int contactId);
    }

    public class ContactRepository : IContactRepository
    {
        private static Db.ContactsContext database = new Db.ContactsContext();

        public IEnumerable<Contact> Contacts
        {
            get
            {
                var contacts = database.Contacts.Include(t => t.ContactPhones);

                var items = contacts.Select(t => new Contact
                {
                    Id = t.ContactId,
                    Name = t.ContactName,
                    DateAdded = t.ContactCreatedDate,
                    Phones = t.ContactPhones.Select(p => new Phone
                    {
                        Number = p.ContactPhoneNumber,
                        PhoneType = (PhoneType)p.ContactPhoneType,

                    }).ToArray(),
                }).ToList();

                return items;
            }
        }

        public void Add(Contact contact)
        {
            try
            {
                var dbContact = new Db.Contact
                {
                    ContactName = contact.Name,
                    ContactPhones = contact.Phones.Select(p => new Db.ContactPhone
                    {
                        ContactPhoneNumber = p.Number,
                        ContactPhoneType = (int)p.PhoneType,
                    }).ToArray(),
                };

                database.Contacts.Add(dbContact);

                database.SaveChanges();

                contact.Id = dbContact.ContactId;
                contact.Name = dbContact.ContactName;
                contact.DateAdded = dbContact.ContactCreatedDate;
                contact.Phones = dbContact.ContactPhones
                    .Select(p => new Phone
                    {
                        Number = p.ContactPhoneNumber,
                        PhoneType = (PhoneType)p.ContactPhoneType,
                    }).ToArray();

            }
            catch (DbUpdateException ex)
            {
                database.Dispose();
                database = new Db.ContactsContext();
                throw;
                //throw new DatabaseException("Missing PhoneNumber")
            }
        }

        public bool Delete(int contactId)
        {
            var contact = database.Contacts.Include(t => t.ContactPhones).FirstOrDefault(t => t.ContactId == contactId);

            if (contact == null)
            {
                return false;
            }
            database.ContactPhones.RemoveRange(contact.ContactPhones);
            database.Contacts.Remove(contact);

            database.SaveChanges();

            return true;
        }

        public bool Update(int contactId, Contact updatedContact)
        {
            var dbContact = database.Contacts.Include(t => t.ContactPhones).FirstOrDefault(t => t.ContactId == contactId);

            if (dbContact == null)
            {
                return false;
            }

            dbContact.ContactName = updatedContact.Name;

            if (updatedContact.Phones != null)
            {
                dbContact.ContactPhones = updatedContact.Phones.Select(p => new Db.ContactPhone
                {
                    ContactPhoneNumber = p.Number,
                    ContactPhoneType = (int)p.PhoneType,
                }).ToArray();
            }

            updatedContact.Id = dbContact.ContactId;
            updatedContact.Name = dbContact.ContactName;
            updatedContact.DateAdded = dbContact.ContactCreatedDate;
            updatedContact.Phones = dbContact.ContactPhones
                .Select(p => new Phone
                {
                    Number = p.ContactPhoneNumber,
                    PhoneType = (PhoneType)p.ContactPhoneType,
                }).ToArray();

            // Automapper version
            //updatedContact = mapper.Map<Contact>(contact);

            database.Contacts.Update(dbContact);

            database.SaveChanges();

            return true;
        }
    }
}
