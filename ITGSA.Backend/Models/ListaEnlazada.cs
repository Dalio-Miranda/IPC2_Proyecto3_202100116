using System.Collections;

namespace ITGSA.Backend.Models
{
    public class ListaEnlazada<T> : IEnumerable<T>
    {
        private Nodo<T>? _cabeza;
        public int Cantidad { get; private set; }

        public void Agregar(T dato)
        {
            var nuevo = new Nodo<T>(dato);
            if (_cabeza == null)
            {
                _cabeza = nuevo;
            }
            else
            {
                var actual = _cabeza;
                while (actual.Siguiente != null)
                    actual = actual.Siguiente;
                actual.Siguiente = nuevo;
            }
            Cantidad++;
        }

        public void Eliminar(T dato)
        {
            if (_cabeza == null) return;
            if (_cabeza.Dato!.Equals(dato))
            {
                _cabeza = _cabeza.Siguiente;
                Cantidad--;
                return;
            }
            var actual = _cabeza;
            while (actual.Siguiente != null)
            {
                if (actual.Siguiente.Dato!.Equals(dato))
                {
                    actual.Siguiente = actual.Siguiente.Siguiente;
                    Cantidad--;
                    return;
                }
                actual = actual.Siguiente;
            }
        }

        public void Limpiar()
        {
            _cabeza = null;
            Cantidad = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var actual = _cabeza;
            while (actual != null)
            {
                yield return actual.Dato;
                actual = actual.Siguiente;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}