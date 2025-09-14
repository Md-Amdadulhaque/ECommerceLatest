using E_commerce.Models;

namespace E_commerce.Interface
{
    public interface IEventPublisher
    {
       public void Publish(EventModel eventModel);
        
    }
}
