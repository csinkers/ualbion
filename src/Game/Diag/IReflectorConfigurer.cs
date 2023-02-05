namespace UAlbion.Game.Diag;

public interface IReflectorConfigurer
{
    ReflectorManager GetManager(Reflector reflector);
    void AssignGetValueFunc(Reflector reflector, Reflector.GetValueDelegate func);
    void AssignSetValueFunc(Reflector reflector, Reflector.SetValueDelegate func);
    void AssignSubObjectsFunc(Reflector reflector, Reflector.VisitChildrenDelegate func);
}