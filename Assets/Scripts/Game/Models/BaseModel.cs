using System;

namespace Models
{
	public abstract class BaseModel
	{
		public readonly string id;

		public BaseModel(string id = null)
		{
			if (id == null)
				this.id = Guid.NewGuid().ToString();
			else
				this.id = id;
		}
	}
}