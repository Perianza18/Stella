# Python prototypes

These pre-Unity prototypes validate each level's mathematics and core behavior. They are reference implementations for the future C# versions, not the foundation of a reusable production puzzle framework.

Each level remains a small, readable console program in its own directory. Core behavior is covered by standard-library regression tests in [`tests/`](tests/).

Run the suite from the repository root:

```bash
python3 -m unittest discover -s prototypes/python/tests -v
```
