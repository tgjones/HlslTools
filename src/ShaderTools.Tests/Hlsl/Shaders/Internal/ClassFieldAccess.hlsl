
class Rng
{
	uint state;
	
	void SetState(uint seed) 
	{
		state = seed; 
	}
};

Rng CreateRng(uint seed)
{
	Rng r;
	r.state = seed; // Field access
	r.SetState(seed); // Method access
	return r;
}
