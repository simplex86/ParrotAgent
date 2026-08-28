# server.py
from fastmcp import FastMCP

mcp = FastMCP("server-calculate")

@mcp.tool
def add(a: int, b: int) -> int:
    return a + b

@mcp.tool
def sub(a: int, b: int) -> int:
    return a - b

if __name__ == "__main__":
    mcp.run(transport="streamable-http", host="127.0.0.1", port=8080)