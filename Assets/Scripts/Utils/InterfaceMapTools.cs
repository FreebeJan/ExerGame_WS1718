using System.Collections.Generic;

namespace UtilsInterface
{
	public interface IKernel
	{
		float ApplyKernel(int x, int y, IEnumerable<Location2D> nodes, float[,] data);
	}

	public interface IFixedSizeArrayIterator<ArrayType>
	{
		ArrayType[] AllocateArray();
	}
}