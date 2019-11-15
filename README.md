# MemoryLeakTest

This project was created to demonstrate a memory leak that occurs when using specific Windows API with C# and .NET Core 3.0.

# The memory leak

For some unkown reason to me when you pass a lambda expression to `EnumThreadWindows` including a variable (For example a List) this List will never get cleared from the memory. (See Option A)
If you pass the list as an handle using GCHandle this issue will not occur.
