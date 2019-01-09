using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;

        }
        public async Task<User> RegisterUser(User user, string password)
        {
            byte[] passwordHarsh, passwordSalt;
            CreatePasswordHash(password, out passwordHarsh, out passwordSalt);

            user.PasswordHash = passwordHarsh;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }
        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Username == username);

            if(user==null){
               return null;
            }

            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)){
                return null;
            }

            return user;
        }

        public async Task<bool> UserExists(string username)
        {
            Boolean existe = false;
            if(await _context.Users.AnyAsync(x => x.Username == username)){
                existe = true;
            }
            return existe;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHarsh, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512()){
                passwordSalt =hmac.Key;
                passwordHarsh = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            Boolean igual = true;
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)){

                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                
                for(int i = 0; i< computedHash.Length; i++){
                    if(computedHash[i]!=passwordHash[i]){
                        igual = false;
                    }
                }
            }
            return igual;
        }
    }
}