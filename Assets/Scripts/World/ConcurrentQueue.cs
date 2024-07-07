using System.Threading;

// TODO Bounds check!
public class ConcurrentQueue<T>
{
	private Mutex m_mtx = new Mutex();
	private T[] m_data;
	private int m_first = 0, m_last = 0;

	public ConcurrentQueue()
	{
		m_data = new T[32];
	}

	public ConcurrentQueue(int size)
	{
		m_data = new T[size];
	}

	public bool CanPop()
	{
		return m_first != m_last;
	}

	public T TryPop()
	{
		if (!CanPop()) // If nothing to pop, return
			return default(T);

		// Value of CanPop might change due to another thread intervening here, so we need
		// to re-check in the protected zone.
		T retval = default(T);
		m_mtx.WaitOne();
		if (CanPop()) // If something to pop, fetch
		{
			retval = m_data[m_first];
			m_first = (m_first + 1) % m_data.Length;
		}
		m_mtx.ReleaseMutex();

		return retval;
	}

	public void Push(T elem)
	{
		m_mtx.WaitOne();
		m_data[m_last] = elem;
		m_last = (m_last + 1) % m_data.Length;
		m_mtx.ReleaseMutex();
	}
}
